![YouTube Comments To File](./Solution%20Items/Images/YouTubeCommentsToFile.gif "YouTube Comments To File")

[Carl Sagan on The Demon-Haunted World and Science l Charlie Rose Rewind](https://www.youtube.com/watch?v=dtCwxFTMMDg "Carl Sagan on The Demon-Haunted World and Science l Charlie Rose Rewind")

# YouTube Comments To File

YouTube Comments To File converts YouTube comments JSON file to a text file or an HTML file. The comments JSON file must be produced by [yt-dlp](https://github.com/yt-dlp/yt-dlp "yt-dlp"). The program doesn't retrieve the comments by itself from YouTube. The output file is aim at readability, showing the comments and replies in a tree-like structure.

The program can also utilize an existing copy of yt-dlp to download and convert comments from YouTube. This mode nullifies the need to execute two scripts, one for yt-dlp, to get the comments JSON file, and another to convert the JSON file to text file or HTML file.

While this program was written with YouTube in mind, it should be applicable for any comment JSON file that yt-dlp can download. Here's a list of [supported sites](https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md "yt-dlp supported sites") by yt-dlp.

YouTube Comments To File requires .NET 8 Runtime.

## Comment Threading

Comment threading is the ability to reply to a reply. A good example is Reddit, where you can have a whole conversation between people in the comments section. YouTube supports threaded comments up to three levels of replies, meaning level 1 top-level comment, level 2 reply to top-level comment, and level 3 reply to reply.

There is a convention on YouTube that if you want to reply to someone else's reply, you write their user name **first** in your reply. This program takes advantage of this convention to build threaded comments for all comments, regardless of YouTube limitation of three levels of replies. This is enabled by default. You can disable comment threading with option [`--disable-threading`](#shared-options "Shared Options"). If disabled, shows the comments the way they appear on YouTube.

## Tutorial

This is a little tutorial how to "Install" this program and use a script to download comments.

1. Download YouTubeCommentsToFile from [Releases](https://github.com/yuvalsol/YouTubeCommentsToFile/releases "YouTubeCommentsToFile Releases").
2. [Download yt-dlp](https://github.com/yt-dlp/yt-dlp#installation "yt-dlp Installation").
3. [Download Deno](https://github.com/denoland/deno "Deno"). For Windows, download deno-x86_64-pc-windows-msvc.zip from [Releases](https://github.com/denoland/deno/releases "Deno Releases").
4. Place YouTubeCommentsToFile, yt-dlp and Deno in the same directory. Make sure that directory **doesn't** need administrative rights (For example, on Windows, **not** under `C:\Program Files`) because you want to update yt-dlp with no prompts.
5. Create this script file [`DownloadComments.cmd`](./Solution%20Items/DownloadComments.cmd "DownloadComments.cmd") in the same directory as YouTubeCommentsToFile. This script downloads comments to HTML file and the comments are sorted by top comments first.

```batch
@echo off
set "url=%~1"
call YouTubeCommentsToFile download --url "%url%" --max-comments 100000 --max-replies-per-thread 100 --sort-top --to-html --show-comment-navigation-links --show-copy-links --uh --trim-title 150 --delete-json-file --update-yt-dlp
exit /b %ERRORLEVEL%
```

- `--url "URL"` Video URL.
- `--max-comments 100000 --max-replies-per-thread 100` At most 100,000 comments and replies, at most 100 replies for each top-level comment.
- `--sort-top` Sort by top comments first.
- `--to-html` Save comments to HTML output file.
- `--show-comment-navigation-links` Show next comment and previous comment navigation links.
- `--show-copy-links` Show copy text links.
- `--uh` Highlight uploader. Comments and replies, written by the uploader of the video, will be highlighted.
- `--trim-title 150` Limit the title of the video to at most 150 characters.
- `--delete-json-file` Delete the comments JSON file after successfully writing to the HTML output file.
- `--update-yt-dlp` Update yt-dlp when using it.

6. Open Command Line/Terminal/shell in the same directory where YouTubeCommentsToFile is at or change the path to it.
7. Download comments with this command. Don't forget to change the URL below.

```console
DownloadComments "https://www.youtube.com/watch?v=dtCwxFTMMDg"
```

8. Repeat steps 6 and 7.

## Convert

Converts comments JSON file to text file or HTML file. Uses verb `convert`. Verb is optional to specify.

```console
YouTubeCommentsToFile [convert]
    --json-file <path>
    [--url <url>]
    [--yt-dlp <path>]
    [--deno <path>]

    [--to-html] [--to-html-and-text] [--dark-theme] [--hide-header]
    [--delete-json-file] [--disable-threading]
    [--hide-comment-separators] [--hide-likes] [--hide-replies] [--hide-time]
    [--hide-video-description] [--indent-size] [--show-comment-link]
    [--show-comment-navigation-links] [--show-copy-links] [--text-line-length]

    [-u] [--uh] [--uf] [--uhf]

    [--ah <authors>] [--af <authors>] [--afi <authors>]
    [--ahf <authors>] [--ahfi <authors>] [-a <authors>] [--ahi <authors>]

    [--th <texts>] [--tf <texts>] [--tfi <texts>] [--thf <texts>] [--thfi <texts>]
    [-t <texts>] [--thi <texts>]

    [--encoding-code-page <codepage>] [--arabic-iso] [--arabic-windows]
    [--baltic-iso-13] [--baltic-iso-4] [--baltic-windows]
    [--central-european] [--chinese-simplified] [--chinese-traditional]
    [--cyrillic-iso] [--cyrillic-windows] [--eastern-european]
    [--greek-iso] [--greek-windows] [--hebrew-iso] [--hebrew-windows]
    [--japanese] [--korean] [--south-european] [--thai] [--turkish-iso]
    [--turkish-windows] [--vietnamese] [--western-european-iso-1]
    [--western-european-iso-15] [--western-european-windows]

    [--text-color <color>] [--background-color <color>] [--author-color <color>]
    [--replied-author-color <color>] [--other-author-color <color>]
    [--small-elements-color <color>] [--favorited-background-color <color>]
    [--header-background-color <color>] [--highlight-uploader-text-color <color>]
    [--highlight-uploader-background-color <color>] [--highlight-uploader-border-color <color>]
    [--highlight-author-text-color <color>] [--highlight-author-background-color <color>]
    [--highlight-author-border-color <color>] [--highlight-text-text-color <color>]
    [--highlight-text-background-color <color>] [--highlight-text-border-color <color>]
    [--link-color <color>] [--copy-text-success-color <color>]
    [--copy-text-failure-color <color>]
```

### Convert Command Line Options

The full path to the comments JSON file. The name of the output file is the same as the name of the JSON file.

```console
--json-file               Path to comments JSON file.
```

Since the video URL can't be extracted from the comments JSON file, by providing it, it makes the output file more robust by writing links to the video, to its comments and replies. URL is not required in order to write to output file.

```console
--url                     Video URL.
```

Path to yt-dlp program. If not set, search for yt-dlp in YouTubeCommentsToFile directory. If not found, check if yt-dlp is accessible in the current environment. It is not necessary to set `--yt-dlp` if yt-dlp is accessible, for example, yt-dlp directory is set in Windows `PATH` environment variable. yt-dlp is used to retrieve information about the video, therefore is not required to write to output file.

```console
--yt-dlp                  Path to yt-dlp.
```

Path to Deno program. If not set, search for Deno in YouTubeCommentsToFile directory. If not found, check if Deno is accessible in the current environment. If Deno is at the same location as yt-dlp, there is no need to specify a path to Deno. To download from YouTube, yt-dlp needs to solve JavaScript challenges presented by YouTube using an external JavaScript runtime. [Deno](https://deno.com "Deno") is a JavaScript runtime best suited for this and yt-dlp enables it by default. [Read more here](https://github.com/yt-dlp/yt-dlp/wiki/EJS "External JS Scripts Setup Guide").

```console
--deno                    Path to Deno.
```

### Convert Examples

1. Convert to text file:

```console
YouTubeCommentsToFile convert --json-file "C:\Path\To\Comments File.json"
```

2. Convert to HTML file:

```console
YouTubeCommentsToFile convert --json-file "C:\Path\To\Comments File.json" --to-html
```

3. Convert to more robust HTML file. By specifying the URL, yt-dlp will download video information - title, uploader, upload date, description:

```console
YouTubeCommentsToFile convert --json-file "C:\Path\To\Comments File.json" --url https://www.youtube.com/watch?v=dtCwxFTMMDg --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --to-html --show-comment-navigation-links --show-copy-links --uh
```

4. Highlight uploader, authors and texts. If a comment or reply matches any of the specified highlight items, the comment is highlighted:

```console
YouTubeCommentsToFile convert --json-file "C:\Path\To\Comments File.json" --uh --ah @Alice Bob --th "Alice and Bob" "are entangled"
```

5. Filter by uploader, authors and texts. If a comment or reply matches any of the specified filter items, it is shown. Otherwise, it is removed:

```console
YouTubeCommentsToFile convert --json-file "C:\Path\To\Comments File.json" --uf --af @Alice Bob --tf "Alice and Bob" "are entangled"
```

## Download

Downloads comments to text file or HTML file. Uses verb `download`. Verb is required to specify.

```console
YouTubeCommentsToFile download
    --url <url>
    [--yt-dlp <path>]
    [--deno <path>]
    [--download-path <path>]
    [--max-comments <value>]
    [--max-parents <value>]
    [--max-replies <value>]
    [--max-replies-per-thread <value>]
    [--sort-new]
    [--sort-top]
    [--filename-sanitization]
    [--restrict-filenames]
    [--windows-filenames]
    [--trim-title]
    [--only-download-comments]
    [--update-yt-dlp]
    [--yt-dlp-options <options>]

    [--to-html] [--to-html-and-text] [--dark-theme] [--hide-header]
    [--delete-json-file] [--disable-threading]
    [--hide-comment-separators] [--hide-likes] [--hide-replies] [--hide-time]
    [--hide-video-description] [--indent-size] [--show-comment-link]
    [--show-comment-navigation-links] [--show-copy-links] [--text-line-length]

    [-u] [--uh] [--uf] [--uhf]

    [--ah <authors>] [--af <authors>] [--afi <authors>]
    [--ahf <authors>] [--ahfi <authors>] [-a <authors>] [--ahi <authors>]

    [--th <texts>] [--tf <texts>] [--tfi <texts>] [--thf <texts>] [--thfi <texts>]
    [-t <texts>] [--thi <texts>]

    [--encoding-code-page <codepage>] [--arabic-iso] [--arabic-windows]
    [--baltic-iso-13] [--baltic-iso-4] [--baltic-windows]
    [--central-european] [--chinese-simplified] [--chinese-traditional]
    [--cyrillic-iso] [--cyrillic-windows] [--eastern-european]
    [--greek-iso] [--greek-windows] [--hebrew-iso] [--hebrew-windows]
    [--japanese] [--korean] [--south-european] [--thai] [--turkish-iso]
    [--turkish-windows] [--vietnamese] [--western-european-iso-1]
    [--western-european-iso-15] [--western-european-windows]

    [--text-color <color>] [--background-color <color>] [--author-color <color>]
    [--replied-author-color <color>] [--other-author-color <color>]
    [--small-elements-color <color>] [--favorited-background-color <color>]
    [--header-background-color <color>] [--highlight-uploader-text-color <color>]
    [--highlight-uploader-background-color <color>] [--highlight-uploader-border-color <color>]
    [--highlight-author-text-color <color>] [--highlight-author-background-color <color>]
    [--highlight-author-border-color <color>] [--highlight-text-text-color <color>]
    [--highlight-text-background-color <color>] [--highlight-text-border-color <color>]
    [--link-color <color>] [--copy-text-success-color <color>]
    [--copy-text-failure-color <color>]
```

YouTubeCommentsToFile builds a yt-dlp command line for downloading comments out of the options provided by the user. Then, it runs yt-dlp to download the comments JSON file. Once the JSON file is downloaded, YouTubeCommentsToFile converts it to a text file or an HTML file or both. If the option is enabled, the JSON file is deleted.

This is the core of yt-dlp command line which downloads comments.

```console
yt-dlp --write-comments
       --print-to-file "%(comments)#+j" "%(uploader&{} - |)s%(title)s.json"
       --skip-download
       --no-write-info-json
```

- `--write-comments` retrieves the video comments.
- `--print-to-file` writes the comments to a file.
- `"%(comments)#+j"` writes the comments as JSON. `#` for pretty-printing, `+` for Unicode, `j` for JSON.
- `"%(uploader&{} - |)s%(title)s.json"` is the JSON file name. `uploader` is the name of the uploader of the video. `title` is the title of the video. If yt-dlp can retrieve the name of the uploader, the file name is `uploader - title.json`. If not, the file name is `title.json`.
- `--skip-download` prevents from downloading the video itself.
- `--no-write-info-json` prevents from writing video metadata file.

### Download Command Line Options

The video to download its comments.

```console
--url                     Video URL to download its comments.
```

Path to yt-dlp program. If not set, search for yt-dlp in YouTubeCommentsToFile directory. If not found, check if yt-dlp is accessible in the current environment. It is not necessary to set `--yt-dlp` if yt-dlp is accessible, for example, yt-dlp directory is set in Windows `PATH` environment variable.

```console
--yt-dlp                  Path to yt-dlp.
```

Path to Deno program. If not set, search for Deno in YouTubeCommentsToFile directory. If not found, check if Deno is accessible in the current environment. If Deno is at the same location as yt-dlp, there is no need to specify a path to Deno. To download from YouTube, yt-dlp needs to solve JavaScript challenges presented by YouTube using an external JavaScript runtime. [Deno](https://deno.com "Deno") is a JavaScript runtime best suited for this and yt-dlp enables it by default. [Read more here](https://github.com/yt-dlp/yt-dlp/wiki/EJS "External JS Scripts Setup Guide").

```console
--deno                    Path to Deno.
```

Path to where to download comments JSON file and convert it to text file or HTML file. If not set, download path is set to YouTubeCommentsToFile directory.

```console
--download-path           Path to download and to convert comments.
```

These options limit the number of comments and replies. If an option is not set, it is unlimited (`all` in yt-dlp). If all options are not set, downloads everything.

```console
--max-comments            Maximum number of top-level comments and replies to extract.
--max-parents             Maximum number of top-level comments to extract.
--max-replies             Maximum number of replies to extract.
--max-replies-per-thread  Maximum number of replies per top-level comment to extract.
```

Example 1: Download up to 10,000 top-level comments and replies, at most 100 replies for each top-level comment.

```console
--max-comments 10000
--max-replies-per-thread 100
```

Example 2: Download all top-level comments, up to 1000 replies, at most 10 replies for each top-level comment.

```console
--max-replies 1000
--max-replies-per-thread 10
```

Example 3: Download up to 1000 top-level comments, no replies.

```console
--max-parents 1000
--max-replies 0
```

Sort comments by new comments first or by top comments first. If no sort option is enabled, yt-dlp default sorting is to order comments by new comments first.

```console
--sort-new                Sort comments by new comments first.
--sort-top                Sort comments by top comments first.
```

These options determine how the file name is processed.

```console
--filename-sanitization   Sanitize Windows reserved characters by removing them or
                          by replacing with comparable character or by replacing
                          with underscore. Doesn't replace with Unicode characters.
--restrict-filenames      Restrict filenames to only ASCII characters,
                          and avoid '&' and spaces in filenames.
--windows-filenames       Force filenames to be Windows-compatible by replacing
                          Windows reserved characters with lookalike Unicode characters.
--trim-title              Limit the length of the video title in the filename
                          to the specified number of characters. Doesn't limit
                          the length of the video uploader.
```

Download the comments JSON file and don't write text and HTML files. This option can be useful if you want to experiment with the options that write to output file but don't want to repeatedly download the video comments. Download the comments once, `YouTubeCommentsToFile download --only-download-comments`, and convert them multiple times, `YouTubeCommentsToFile convert --json-file`.

```console
--only-download-comments  Whether to download comments JSON file and stop.
```

Update yt-dlp before downloading comments. Updates to `nightly` release channel. YouTube and yt-dlp developers are engaged in a cat and mouse chase, YouTube being the mouse and yt-dlp being the cat. YouTube is making frequent changes to the site in order to curve, limit and stop scraping tools such as yt-dlp. The developers of yt-dlp are quick to respond with changes to the program, but as a result of that, the users can no longer update yt-dlp once in a-very-long-time. If yt-dlp is not frequently updated (daily, weekly) to the newest version, it's more than likely it will stop working properly. Hence the update option. This option automates the updating process and takes the burden of remembering to update the program off the user's shoulders.

```console
--update-yt-dlp           Whether to update yt-dlp to the latest version.
```

Options for yt-dlp which are added to yt-dlp command line. This is for advanced yt-dlp features such as `--config-locations`, `--cookies`, `--cookies-from-browser`.

```console
--yt-dlp-options          Options added to yt-dlp command line.
```

Quick reminder, double quotes in double quotes need to be escaped with a backslash.

```console
--yt-dlp-options "--config-locations \"C:\A Path\With\A Space\yt-dlp.conf\" --cookies cookies.txt"
```

### Download Examples

1. Download to text file:

```console
YouTubeCommentsToFile download --url https://www.youtube.com/watch?v=dtCwxFTMMDg --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder"
```

2. Download to HTML file:

```console
YouTubeCommentsToFile download --url https://www.youtube.com/watch?v=dtCwxFTMMDg --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --to-html
```

3. Download to more robust HTML file:

```console
YouTubeCommentsToFile download --url https://www.youtube.com/watch?v=dtCwxFTMMDg --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --to-html --show-comment-navigation-links --show-copy-links --uh
```

4. The `--yt-dlp` option can be omitted if yt-dlp is at the same directory as YouTubeCommentsToFile:

```console
YouTubeCommentsToFile download --url https://www.youtube.com/watch?v=dtCwxFTMMDg --download-path "C:\Path\To\Downloads Folder"
```

5. Download all comments, sort by top comments first:

```console
YouTubeCommentsToFile download --url https://www.youtube.com/watch?v=dtCwxFTMMDg --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --sort-top
```

6. Download up to 10,000 top-level comments and replies, at most 100 replies for each top-level comment, sort by new comments first (default):

```console
YouTubeCommentsToFile download --url https://www.youtube.com/watch?v=dtCwxFTMMDg --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --max-comments 10000 --max-replies-per-thread 100
```

7. Download all top-level comments, up to 1000 replies, at most 10 replies for each top-level comment, sort by new comments first:

```console
YouTubeCommentsToFile download --url https://www.youtube.com/watch?v=dtCwxFTMMDg --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --max-replies 1000 --max-replies-per-thread 10 --sort-new
```

8. Download up to 1000 top-level comments, no replies, sort by top comments first:

```console
YouTubeCommentsToFile download --url https://www.youtube.com/watch?v=dtCwxFTMMDg --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --max-parents 1000 --max-replies 0 --sort-top
```

9. Download to text file, use configuration file for yt-dlp, use cookies file for restricted video:

```console
YouTubeCommentsToFile download --url https://www.youtube.com/watch?v=dtCwxFTMMDg --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --yt-dlp-options "--config-locations \"C:\Path\To\yt-dlp.conf\" --cookies cookies.txt"
```

## Convert and Download Shared Options

These options determine whether to write a text file, an HTML file or both. They also determine the structure of the output file.

### Shared Options

When both `--to-html` and `--to-html-and-text` are disabled, writes only to text file.

```console
--to-html                        Whether to write comments to HTML file.
--to-html-and-text               Whether to write comments to HTML file
                                 and to text file.
```

Disable [comment threading](#comment-threading "Comment Threading"). Shows the comments the way they appear on YouTube.

```console
--disable-threading              Whether to disable comment threading.
```

Hide elements from the output file.

```console
--hide-comment-separators        Whether to hide separators between top-level comments.
--hide-header                    Whether to hide the video information header,
                                 containing title, URL, uploader, description.
--hide-likes                     Whether to hide likes count.
--hide-replies                   Whether to hide replies and leave only top-level comments.
--hide-time                      Whether to hide the posted time of the comment or reply.
--hide-video-description         Whether to hide the video description from the
                                 video information header.
```

This option creates links to comments and replies. It is useful if you want to read the comments offline and when you decide to reply to someone's comment, the link will take you directly to it instead of searching for it on the video page. In `convert` mode, if `--show-comment-link` is enabled and the video URL (`--url`) is not provided, shows only the part of the URL that points to a comment (`&lc=` for YouTube). Then, you can copy that and add it to the video URL in the browser.

```console
--show-comment-link              Whether to show direct links comments and replies.
```

These options determine the structure of the HTML output file.

```console
--dark-theme                     Whether to use dark theme in HTML file.
--show-comment-navigation-links  Whether to show next comment and previous
                                 comment navigation links in HTML file.
--show-copy-links                Whether to show copy text links for comments
                                 and replies in HTML file.
```

This option determines how much characters of indentation are between a comment and a reply or between a reply to another reply. Indentation size is between 2 and 10 characters. If this option is not set, indentation for text file and HTML file is 4 characters.

```console
--indent-size                    The indentation size, in number of characters.
```

This option determines how many characters a single line of text is allowed. If a comment has a line of text that is longer than that, the line is split into several lines which adhere to that length limitation. Line length is between 80 and 320 characters. If this option is not set, line length for text file is 120 characters and line length for HTML file is 150 characters.

```console
--text-line-length               The maximum length of a line of text,
                                 in number of characters.
```

Delete JSON file after comments were written successfully to the output file.

```console
--delete-json-file               Whether to delete the comments JSON file
                                 on successful completion.
```

### Uploader

Highlight uploader: Search for comments and replies written by the uploader of the video. If found, highlight the comment or reply.

Filter uploader: Search for comments and replies written by the uploader of the video. If found, keep the conversation (comment and its replies). Otherwise, remove the conversation, if it didn't match any other filter items.

```console
-u, --uh     Highlight uploader.
--uf         Filter uploader.
--uhf        Highlight and filter uploader.
```

#### Uploader Examples

1. Highlight all comments written by the uploader of the video:

```console
YouTubeCommentsToFile download --uh
```

2. Show only conversations that the uploader of the video participated in:

```console
YouTubeCommentsToFile download --uf
```

### Authors

Highlight authors: Search for comments and replies written by **any** of the specified authors. If found, highlight the comment or reply.

Filter authors: Search for comments and replies written by **any** of the specified authors. If found, keep the conversation (comment and its replies). Otherwise, remove the conversation, if it didn't match any other filter items.

Author name can be written with or without a leading @.

```console
--ah         Highlight authors.
--af         Filter authors.
--afi        Filter authors. Author search is case-insensitive.
--ahf        Highlight and filter authors.
--ahfi       Highlight and filter authors. Author search is case-insensitive.
-a, --ahi    Highlight authors. Author search is case-insensitive.
```

#### Authors Examples

1. Highlight all comments written by either `@Alice` or `@Bob`. Search for author names is case sensitive:

```console
YouTubeCommentsToFile download --ah @Alice Bob
```

2. Highlight all comments written by either `@Alice` or `@Bob`. Search for author names is case insensitive:

```console
YouTubeCommentsToFile download --ahi alice @bob
```

3. Show only conversations that either `@Alice` or `@Bob` participated in. Search for author names is case insensitive:

```console
YouTubeCommentsToFile download --afi alice bob
```

### Texts

Highlight texts: Search for **any** of the specified texts in the comment or reply. If found, highlight the comment or reply.

Filter texts: Search for **any** of the specified texts in the comment or reply. If found, keep the conversation (comment and its replies). Otherwise, remove the conversation, if it didn't match any other filter items.

```console
--th         Highlight texts.
--tf         Filter texts.
--tfi        Filter texts. Text search is case-insensitive.
--thf        Highlight and filter texts.
--thfi       Highlight and filter texts. Text search is case-insensitive.
-t, --thi    Highlight texts. Text search is case-insensitive.
```

#### Texts Examples

1. Highlight all comments that contain any of the text phrases `Alice and Bob` and `are entangled`. Search for text phrases is case sensitive:

```console
YouTubeCommentsToFile download --th "Alice and Bob" "are entangled"
```

2. Highlight all comments that contain any of the text phrases `Alice and Bob` and `are entangled`. Search for text phrases is case insensitive:

```console
YouTubeCommentsToFile download --thi "alice and bob" "are entangled"
```

3. Show only conversations with comments that contain any of the text phrases `Alice and Bob` and `are entangled`. Search for text phrases is case sensitive:

```console
YouTubeCommentsToFile download --tf "Alice and Bob" "are entangled"
```

### Encoding

If the video page is not in English (the title, the description, majority of the comments) and downloading comments fails or comes out garbled, use this option to change the encoding. You need to find beforehand what is the code page of the non-English language.

```console
--encoding-code-page          The code page of the video page encoding.
```

Or, use one of these predefined encodings.

```console
--arabic-iso                  Arabic (ISO-8859-6). Code page 28596.
                              Arabic, Persian, Urdu.
--arabic-windows              Arabic (Windows-1256). Code page 1256.
--baltic-iso-13               Baltic (ISO-8859-13). Code page 28603.
                              Lithuanian, Latvian, Estonian.
--baltic-iso-4                Baltic (ISO-8859-4). Code page 28594.
                              Estonian, Latvian, Lithuanian, Greenlandic, Sami.
--baltic-windows              Baltic (Windows-1257). Code page 1257.
--central-european            Central European (Windows-1250). Code page 1250.
--chinese-simplified          Chinese Simplified (GB2312). Code page 936.
--chinese-traditional         Chinese Traditional (Big5). Code page 950.
--cyrillic-iso                Cyrillic (ISO-8859-5). Code page 28595.
                              Russian, Ukrainian, Bulgarian, Belarusian,
                              Macedonian, Serbian.
--cyrillic-windows            Cyrillic (Windows-1251). Code page 1251.
--eastern-european            Eastern European (ISO-8859-2). Code page 28592.
                              Czech, Hungarian, Polish, Romanian, Croatian,
                              Slovak, Slovenian.
--greek-iso                   Greek (ISO-8859-7). Code page 28597.
--greek-windows               Greek (Windows-1253). Code page 1253.
--hebrew-iso                  Hebrew (ISO-8859-8). Code page 28598.
--hebrew-windows              Hebrew (Windows-1255). Code page 1255.
--japanese                    Japanese (Shift-JIS). Code page 932.
--korean                      Korean (Windows-949). Code page 949.
--south-european              South European (ISO-8859-3). Code page 28593.
                              Maltese, Esperanto, Turkish.
--thai                        Thai (TIS-620). Code page 874.
--turkish-iso                 Turkish (ISO-8859-9). Code page 28599.
--turkish-windows             Turkish (Windows-1254). Code page 1254.
--vietnamese                  Vietnamese (Windows-1258). Code page 1258.
--western-european-iso-1      Western European (ISO-8859-1). Code page 28591.
                              English, French, Spanish, German, Italian, Portuguese.
--western-european-iso-15     Western European (ISO-8859-15). Code page 28605.
                              English, French, German, Spanish, Finnish, Danish,
                              Swedish, Italian, Dutch.
--western-european-windows    Western European (Windows-1252). Code page 1252.
```

### HTML Colors

These options set the colors of various HTML elements. A color value can be a known name (`Honeydew`), hex code (`#F0FFF0`; `F0FFF0`) or RGB (`240, 255, 240`; `240 255 240`).

```console
--text-color                            The text color of comments.
--background-color                      The background color of comments.
--author-color                          The text color of the author of the comment.
--replied-author-color                  The text color of an author, of other comment,
                                        appearing at the start of the comment.
--other-author-color                    The text color of an author, of other comment,
                                        appearing anywhere else but the start of the comment.
--small-elements-color                  The text color of small elements, such as
                                        pinned comment, comment time, likes.
--favorited-background-color            The background color of favorited by the
                                        uploader of the video.
--header-background-color               The background color of the header.
--highlight-uploader-text-color         The text color of highlighted comments
                                        written by the uploader of the video.
--highlight-uploader-background-color   The background color of highlighted comments
                                        written by the uploader of the video.
--highlight-uploader-border-color       The color of the border around highlighted comments
                                        written by the uploader of the video.
--highlight-author-text-color           The text color of highlighted comments
                                        written by any of the searched authors.
--highlight-author-background-color     The background color of highlighted comments
                                        written by any of the searched authors.
--highlight-author-border-color         The color of the border around highlighted comments
                                        written by any of the searched authors.
--highlight-text-text-color             The text color of highlighted comments
                                        containing any of the searched texts.
--highlight-text-background-color       The background color of highlighted comments
                                        containing any of the searched texts.
--highlight-text-border-color           The color of the border around highlighted comments
                                        containing any of the searched texts.
--link-color                            The text color of HTML links.
--copy-text-success-color               The text color of a success message after copying
                                        a comment to the clipboard.
--copy-text-failure-color               The text color of an error message after failing
                                        to copy a comment to the clipboard.
```

The colors of the HTML elements are determined by light theme (default theme) or dark theme (`--dark-theme`).
The following are the colors of these themes.

| Light Theme                    | RGB           | Hex     |
|--------------------------------|---------------|---------|
| Text                           | 15, 15, 15    | #0F0F0F |
| Background                     | 255, 255, 255 | #FFFFFF |
| Author                         | 15, 15, 15    | #0F0F0F |
| Replied Author                 | 15, 128, 15   | #0F800F |
| Other Author                   | 15, 15, 255   | #0F0FFF |
| Small Elements                 | 96, 96, 96    | #606060 |
| Favorited Background           | 229, 235, 238 | #E5EBEE |
| Header Background              | 242, 242, 242 | #F2F2F2 |
| Highlight Uploader: Text       | 15, 15, 15    | #0F0F0F |
| Highlight Uploader: Background | 255, 250, 238 | #FFFAEE |
| Highlight Uploader: Border     | 183, 136, 0   | #B78800 |
| Highlight Author: Text         | 15, 15, 15    | #0F0F0F |
| Highlight Author: Background   | 252, 241, 237 | #FCF1ED |
| Highlight Author: Border       | 255, 67, 5    | #FF4305 |
| Highlight Text: Text           | 15, 15, 15    | #0F0F0F |
| Highlight Text: Background     | 231, 239, 252 | #E7EFFC |
| Highlight Text: Border         | 34, 110, 233  | #226EE9 |
| Link                           | 0, 0, 238     | #0000EE |
| Copy Text: Success             | 15, 170, 15   | #0FAA0F |
| Copy Text: Failure             | 255, 15, 15   | #FF0F0F |

| Dark Theme                     | RGB           | Hex     |
|--------------------------------|---------------|---------|
| Text                           | 241, 241, 241 | #F1F1F1 |
| Background                     | 15, 15, 15    | #0F0F0F |
| Author                         | 241, 241, 241 | #F1F1F1 |
| Replied Author                 | 15, 128, 15   | #0F800F |
| Other Author                   | 191, 15, 255  | #BF0FFF |
| Small Elements                 | 170, 170, 170 | #AAAAAA |
| Favorited Background           | 96, 96, 96    | #606060 |
| Header Background              | 39, 39, 39    | #272727 |
| Highlight Uploader: Text       | 241, 241, 241 | #F1F1F1 |
| Highlight Uploader: Background | 140, 33, 23   | #8C2117 |
| Highlight Uploader: Border     | 252, 225, 150 | #FCE196 |
| Highlight Author: Text         | 241, 241, 241 | #F1F1F1 |
| Highlight Author: Background   | 137, 57, 16   | #893910 |
| Highlight Author: Border       | 255, 165, 0   | #FFA500 |
| Highlight Text: Text           | 241, 241, 241 | #F1F1F1 |
| Highlight Text: Background     | 13, 57, 132   | #0D3984 |
| Highlight Text: Border         | 231, 239, 252 | #E7EFFC |
| Link                           | 62, 166, 255  | #3EA6FF |
| Copy Text: Success             | 15, 170, 15   | #0FAA0F |
| Copy Text: Failure             | 213, 15, 15   | #D50F0F |

## Acknowledgments

[yt-dlp](https://github.com/yt-dlp/yt-dlp "yt-dlp")\
[Comment icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/comment "comment icons")
