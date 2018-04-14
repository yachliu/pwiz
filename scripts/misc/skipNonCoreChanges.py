import os
import sys
import subprocess
import re

def runCommandAndExit(args):
        cmd = args[0]
        args = args[0:len(args)]
        os.execv(cmd, args)
        exit(0)

args = sys.argv[1:len(sys.argv)]

if len(args) == 0:
    print("Usage:")
    print(" %s <command to run if any changed path in current git branch is a core path> [<arg1> [<arg2> ...]]" % os.path.basename(sys.argv[0]))
    exit(0)

matchPaths = \
[
    "pwiz_tools/Bumbershoot/.*",
    "pwiz_tools/Skyline/.*",
    "pwiz_tools/Topograph/.*",
    "pwiz_tools/Shared/.*"
]

branch = subprocess.check_output("git branch", shell=True).decode(sys.stdout.encoding)
print("Branches:\n", branch)
branch = re.search("(?<=\* )([^\n]*)", branch).groups(0)[0]
print("Current branch: %s" % branch)
if branch == "master":
    runCommandAndExit(args)

changed_files = subprocess.check_output("git whatchanged --name-only --pretty=\"\" master..HEAD", shell=True).decode(sys.stdout.encoding)
print("Changed files:\n", changed_files)
changed_files = changed_files.splitlines()
pathsPattern = "(?:" + ")|(?:".join(matchPaths) + ")"

# if any changed file does not match to one of the paths above, then we run the command
for path in changed_files:
    if not re.match(pathsPattern, path):
        print("Core path triggering build: %s" % path)
        runCommandAndExit(args)

# otherwise we don't run it but still report success
