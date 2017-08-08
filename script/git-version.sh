#!/bin/bash

########################################
# Helpers
########################################

REGEX_SEMVER="^v([0-9]+)\.([0-9]+)\.([0-9]+)$"
REGEX_BRANCH="^[a-z/]+/(.*)$"
REGEX_VERSION_SPLIT="^([0-9]+\.[0-9]+\.[0-9]+)([-+].*)?$"
DIR_ROOT="$(git rev-parse --show-toplevel 2> /dev/null)"
CHANGELOG="CHANGELOG.md"

# Use in the the functions: eval $invocation
invocation='say_verbose "Calling: ${yellow:-}${FUNCNAME[0]} ${green:-}$*${normal:-}"'

# standard output may be used as a return value in the functions
# we need a way to write text on the screen in the functions so that
# it won't interfere with the return value.
# Exposing stream 3 as a pipe to standard output of the script itself
exec 3>&1

# Setup some colors to use. These need to work in fairly limited shells, like the Ubuntu Docker container where there are only 8 colors.
# See if stdout is a terminal
if [ -t 1 ]; then
    # see if it supports colors
  ncolors=$(tput colors)
  if [ -n "$ncolors" ] && [ $ncolors -ge 8 ]; then
    bold="$(tput bold       || echo)"
    normal="$(tput sgr0     || echo)"
    black="$(tput setaf 0   || echo)"
    red="$(tput setaf 1     || echo)"
    green="$(tput setaf 2   || echo)"
    yellow="$(tput setaf 3  || echo)"
    blue="$(tput setaf 4    || echo)"
    magenta="$(tput setaf 5 || echo)"
    cyan="$(tput setaf 6    || echo)"
    white="$(tput setaf 7   || echo)"
  fi
fi

function say_err() {
  printf "%b\n" "${red:-}git-version: Error: $1${normal:-}" >&2
}

function say() {
  # using stream 3 (defined in the beginning) to not interfere with stdout of functions
  # which may be used as return value
  printf "%b\n" "${cyan:-}git-version:${normal:-} $1" >&3
}

function say_verbose() {
  if [ "$verbose" = true ]; then
    say "$1"
  fi
}

function say_set() {
  local varname="$1"
  local value="$2"

  say_verbose "${green:-}$varname${normal:-}=${yellow}$value${normal:-}"
}

# Joins elements in an array with a separator
# Takes a separator and array of elements to join
#
# Adapted from code by gniourf_gniourf (http://stackoverflow.com/a/23673883/1819350)
#
# Example
#   $ arr=("red car" "blue bike")
#   $ join " and " "${arr[@]}"
#   red car and blue bike
#   $ join $'\n' "${arr[@]}"
#   red car
#   blue bike
#
function join() {
  local separator=$1
  local elements=$2
  shift 2 || shift $(($#))
  printf "%s" "$elements${@/#/$separator}"
}

# Resolves a path to a real path
# Takes a string path
#
# Example
#   $ echo $(resolve-path "/var/./www/../log/messages.log")
#   /var/log/messages.log
#
function resolve-path() {
  local path="$1"
  if pushd "$path" > /dev/null 2>&1
  then
    path=$(pwd -P)
    popd > /dev/null
  elif [ -L "$path" ]
  then
    path="$(ls -l "$path" | sed 's#.* /#/#g')"
    path="$(resolve-path $(dirname "$path"))/$(basename "$path")"
  fi
  echo "$path"
}

function basename-git() {
  echo $(basename "$1" | tr '-' ' ' | sed 's/.sh$//g')
}

########################################
# Git version functions
########################################
function get-prev-version-tag() {
  eval $invocation

  local tag=$(git describe --tags --abbrev=0 --match="v[0-9]*" 2> /dev/null)
  if ! [[ $? -eq 0 ]]; then
    #local err=$((git describe --tags --abbrev=0 --match="v[0-9]*" 1> /dev/null) 2>&1)
    local err=$(git describe --tags --abbrev=0 --match="v[0-9]*" 1> /dev/null 2>&1)
    say_verbose "${magenta:-}No version tags found${normal:-}"
    say_verbose "Get tags error: $err"
    echo ""
    return
  fi

  until [[ "$tag" =~ $REGEX_SEMVER ]]; do
    say_verbose "git describe --tags --abbrev=0 --match=\"v[0-9]*\""
    git describe --tags --abbrev=0 --match="v[0-9]*" >&3
    say_verbose "$tag is not a valid version tag, looking for the next one..."
    tag=$(git describe --tags --abbrev=0 --match="v[0-9]*" "$tag^" 2> /dev/null)
    if ! [[ $? -eq 0 ]]; then
      local err=$(git describe --tags --abbrev=0 --match="v[0-9]*" 1> /dev/null 2>&1)
      say_verbose "${magenta:-}No version tags found${normal:-}"
      say_verbose "Get tags error: $err"
      echo ""
      return
    fi
  done

  echo "$tag"
}

function get-version-from-tag() {
  eval $invocation

  local tag="$1"
  if [[ "$tag" =~ $REGEX_SEMVER ]]; then
    local major=${BASH_REMATCH[1]}
    local minor=${BASH_REMATCH[2]}
    local patch=${BASH_REMATCH[3]}

    echo "$major.$minor.$patch"
    return
  fi

  say_err "$tag is not a valid version"
  exit 1
}

function get-exact-version-tag() {
  eval $invocation

  local tag=$(git describe --exact-match --tags --abbrev=0 --match="v[0-9]*" HEAD 2> /dev/null)
  if [[ $? -eq 0 ]]; then
    if [[ "$tag" =~ $REGEX_SEMVER ]]; then
      local major=${BASH_REMATCH[1]}
      local minor=${BASH_REMATCH[2]}
      local patch=${BASH_REMATCH[3]}

      echo "$major.$minor.$patch"
      return
    fi
  fi

  echo ""
}

function get-next-full-version() {
  eval $invocation

  local tag="$1"
  if [[ "$tag" =~ $REGEX_SEMVER ]]; then
    local major=${BASH_REMATCH[1]}
    local minor=${BASH_REMATCH[2]}
    local patch=${BASH_REMATCH[3]}

    local incrMajor=$(git rev-list --count --grep="Semver: major" $tag..HEAD)
    local incrMinor=$(git rev-list --count --grep="Semver: minor" $tag..HEAD)

    if [[ $incrMajor > 0 ]]; then
      say_verbose "incrementing major"
      major=$(($major + 1))
      minor=0
      patch=0
    elif [[ $incrMinor > 0 ]]; then
      say_verbose "incrementing minor"
      minor=$(($minor + 1))
      patch=0
    else
      patch=$(($patch + 1))
    fi

    echo "$major.$minor.$patch"
    return
  fi

  echo "0.1.0"
}

function get-branch-name() {
  eval $invocation

  git rev-parse --abbrev-ref HEAD
}

function get-branch-short-name() {
  eval $invocation

  local branch=$1

  if [[ "$branch" =~ $REGEX_BRANCH ]]; then
    branch=${BASH_REMATCH[1]}
  else
    say_verbose "did not match"
  fi

  echo "$branch"
}

function get-branch-point() {
  eval $invocation

  local branch=$1
  local base=$2
  diff -u <(git rev-list --first-parent "$branch") <(git rev-list --first-parent "$base") | sed -ne 's/^ //p' | head -1
}

function get-commit-count() {
  eval $invocation

  local since="$1"
  if [[ "$since" == "" ]]; then
    git rev-list --count HEAD
  else
    git rev-list --count "$since"..HEAD
  fi
}

function get-git-short-hash() {
  git rev-parse --short HEAD
}

function get-branch-version-meta() {
  eval $invocation

  local lastVersionTag="$1"

  local branchName=$(get-branch-name)
  say_set "branch-name" "$branchName"

  if [[ "$branchName" == "master" ]]; then
    local commitCount=$(get-commit-count "$lastVersionTag")
    say_set "commit-count" "$commitCount"
    echo "-ci.$commitCount"
    return
  fi

  local shortName=$(get-branch-short-name "$branchName")
  say_set "short-name" "$shortName"

  local branchPoint=$(get-branch-point "$branchName" "master")
  say_set "branch-point" "$branchPoint"

  local commitCount=$(get-commit-count "$branchPoint")
  say_set "commit-count" "$commitCount"

  local shortHash=$(get-git-short-hash)
  say_set "short-hash" "$shortHash"

  echo "-$shortName.$commitCount+$shortHash"
}

function get-version() {
  eval $invocation

  local exact=$(get-exact-version-tag)
  if ! [[ "$exact" == "" ]]; then
    say_verbose "Exact version match!"
    echo "$exact"
    return
  fi

  local lastVersionTag=$(get-prev-version-tag)
  say_set "last-version-tag" "$lastVersionTag"
  local nextVersion=$(get-next-full-version "$lastVersionTag")
  say_set "next-full-version" "$nextVersion"
  local branchMeta=$(get-branch-version-meta "$lastVersionTag")
  say_set "branch-meta" "$branchMeta"
  echo "$nextVersion$branchMeta"
}

function create-version-props() {
  eval $invocation
  local version_string=$(get-version)
  say_set "version_string" "$version"

  # if [[ "$version_string" =~ $REGEX_VERSION_SPLIT ]]; then
  #   local prefix=${BASH_REMATCH[1]}
  #   local suffix=${BASH_REMATCH[2]}

  #   say_set "prefix" "$prefix"
  #   say_set "suffix" "$suffix"

  #   echo "<!-- This file may be overwritten by automation. Only values allowed here are VersionPrefix and VersionSuffix.  -->"
  #   echo "<Project>"
  #   echo "    <PropertyGroup>"
  #   echo "        <VersionPrefix>$prefix</VersionPrefix>"
  #   echo "        <VersionSuffix>$suffix</VersionSuffix>"
  #   echo "    </PropertyGroup>"
  #   echo "</Project>"
  # else
  #   say_err "Version does not fit regex (script error)."
  #   exit 1
  # fi

  echo "<!-- This file may be overwritten by automation. Only values allowed here are VersionPrefix and VersionSuffix.  -->"
  echo "<Project>"
  echo "    <PropertyGroup>"
  echo "        <Version>$version_string</Version>"
  echo "    </PropertyGroup>"
  echo "</Project>"
}

function update-changelog() {
  local lastVersionTag=$(get-prev-version-tag)
  say_set "last-version-tag" "$lastVersionTag"
  local nextVersion=$(get-next-full-version "$lastVersionTag")
  say_set "next-full-version" "$nextVersion"

  cp package.json package.json.tmp
  jq ".version=\"$nextVersion\"" < package.json.tmp > package.json
  ./node_modules/.bin/conventional-changelog -i CHANGELOG.md -s
  rm package.json
  mv package.json.tmp package.json
}

function add-changelog-to-git() {
  git add CHANGELOG.md
}

function create-release-commit() {
  local lastVersionTag=$(get-prev-version-tag)
  say_set "last-version-tag" "$lastVersionTag"
  local nextVersion=$(get-next-full-version "$lastVersionTag")
  say_set "next-full-version" "$nextVersion"

  git commit -m "Release: v$nextVersion"
}

function create-version-tag() {
  local lastVersionTag=$(get-prev-version-tag)
  say_set "last-version-tag" "$lastVersionTag"
  local nextVersion=$(get-next-full-version "$lastVersionTag")
  say_set "next-full-version" "$nextVersion"

  git tag "v$nextVersion"
}

verbose=false

if [[ $# -eq 0 ]]; then
  echo "Use sub-command get or release"
  exit 1
fi

while [ $# -ne 0 ]; do
  name=$1
  case $name in
    --verbose)
      verbose=true
      ;;

    --dir)
      pushd "$2" > /dev/null
      shift
      ;;

    get)
      get-version
      ;;

    version-props)
      create-version-props
      ;;

    changelog)
      update-changelog
      ;;

    release)
      update-changelog
      add-changelog-to-git
      create-release-commit
      create-version-tag
      ;;

    tag)
      create-version-tag
      ;;

    *)
      say_err "Unknown argument \"${red:-}$name${normal:-}\""
  esac
  shift
done
