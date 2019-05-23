# ContentSync
Directory copy/sync/mirror tool that uses file contents (not timestamps) to avoid touching identical files.

## Download

https://github.com/KirillOsenkov/ContentSync/releases/latest/download/ContentSync.exe

Also available on Chocolatey:
https://chocolatey.org/packages/ContentSync/

    cinst ContentSync

## Usage

```
    Usage: ContentSync.exe <source> <destination> [<pattern>] [-c] [-u] [-d]
                                                              [-dc] [-ds]
                                                              [-whatif] [-q]
                                                              [-h]

    Copy/mirror/sync the destination folder to look exactly like the source.
    Only modifies the destination folder, never touches the source folder.
    Copies missing files from source to destination. Overwrites files in
    destination that have different contents than in source. Deletes files from
    destination that are not in source.
    
    Doesn't take into account file and date timestamps, works off content only.

    -c       Copy files from source that don't exist in destination (left-only).

    -u       Update files that have changed between source and destination. This
             only overwrites the destination file if the contents are different.

    -d       Delete right-only files (that are in destination but not in
             source).

    -ds      Delete same (identical) files from destination (that exist in
             source and destination and have identical contents).

    -dc      Delete changed files from destination (can't be used with -u).

    -whatif  Print what would have been done (without changing anything).

    -q       Quiet mode. Do not output anything to the console.

    -h       Display this help and exit.

    Default is: -c -u -d (and if the pattern is not specified, it also syncs
    empty directories (creates empty directories that are in the source and
    deletes empty directories that are in the destination).

    Explicit mode: If any of -c -u -d -ds or -dc are specified explicitly,
    all the defaults for other arguments are reset to false. For instance
    if you specify -u then -c and -d default to false (and you have to
    specify them explicitly). But if no arguments are specified, -c -u -d
    default to true.
    
    You can combine -c and -u to -cu, -c and -d to -cd, -u and -d to -ud
    and -c -u -d to -cud.

    Common usage examples:
    -c to copy files missing from the destination (useful when restarting a previous failed copy operation)
    -cu -whatif to see which files have changed or are new
    -ds to delete duplicate (identical files) from destination
    -whatif to diff directories
```

**Important!** Use at your own risk.

## Overview

Suppose you need to sync the contents of two large folders, Source and Destination. Normally ```robocopy *.* Source Destination /MIR``` does the job. However even if the byte content of a file didn't change, but the timestamp did, robocopy will copy the file and change the timestamp of the destination to match the source.

This is the safe and expected behavior, however there are whole classes of tools that track file modification based off the timestamp alone. For instance, MSBuild will trigger a cascading rebuild of all dependencies of a file if its timestamp has changed (even if the actual file contents is exactly the same). This is called overbuilding. A scalable build system should detect that the file contents didn’t change and avoid doing any work in this case.

Or take another example, suppose you need to upload hundreds of thousands of files to Azure using MSDeploy. If the timestamp on those files has changed, MSDeploy will upload those files even if the actual content is the same.

In general, if you’re deciding whether a file was modified based off the timestamp, you’re bound to schedule unnecessary work that could have been avoided if you checked whether the actual file bytes have changed.

This tool synchronizes the directories based on file contents and ignores timestamps completely.

## How it works
It's quite simple. First it diffs the source and destination directories and builds a flat list of files 
 1) present only on the left, 
 2) present only on the right, 
 3) changed between left and right and 
 4) fully identical (having same bytes). 

The algorithm is non-recursive (I hide the recursion by calling ```Directory.GetFiles()``` to establish the initial flat lists of files on the left and on the right.

The tool also syncs empty folders although it's kind of a bolt-on. Since I only built a list of files, I don't have the knowledge about the empty directories on the left and right. So (yeah, yuck) I traverse the left and right again, this time collecting all the folders on the left and all the folders on the right. Then I diff those as well.

Finally when I have the diff ready, I begin the second phase which actually does stuff to your file system. I 
 1) copy all the left-only files, 
 2) copy over all the changed files and 
 3) delete all the right-only files. Then I 
 4) create all empty directories that are left-only and 
 5) delete all the empty directories which are right-only.

## Check out the tests
I took an opportunity to write the unit-tests in a declarative, data-driven style where there's no explicit arrange-act-assert. Instead, both the Act and Assert phases are implicit (the tests are self-verifying). All you need to specify is the initial state of the left and right folders, the test does the rest (it Acts, and then compares the left and right folders to make sure they're identical).

Read more here:

https://github.com/KirillOsenkov/ContentSync/blob/master/src/ContentSync.Tests/Tests.cs
https://github.com/KirillOsenkov/ContentSync/blob/master/src/ContentSync.Tests/Folder.cs
