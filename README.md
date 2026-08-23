![YouTube Comments To File](./Solution%20Items/Images/YouTubeCommentsToFile.gif "YouTube Comments To File")

[Carl Sagan on The Demon-Haunted World and Science l Charlie Rose Rewind](https://www.youtube.com/watch?v=dtCwxFTMMDg "Carl Sagan on The Demon-Haunted World and Science l Charlie Rose Rewind")

# YouTube Comments To File

YouTube Comments To File converts YouTube comments JSON file to a text file or an HTML file. The comments JSON file must be produced by [yt-dlp](https://github.com/yt-dlp/yt-dlp "yt-dlp"). The program doesn't retrieve the comments by itself from YouTube. The output file is aim at readability, showing the comments and replies in a tree-like structure.

The program can also utilize an existing copy of yt-dlp to download and convert comments from YouTube. This mode nullifies the need to execute two scripts, one for yt-dlp, to get the comments JSON file, and another to convert the JSON file to text file or HTML file.

While this program was written with YouTube in mind, it should be applicable for any comment JSON file that yt-dlp can download, one of which is BiliBili. Here's a list of [supported sites](https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md "yt-dlp supported sites") by yt-dlp.

YouTube Comments To File requires .NET 8 Runtime.

## Comment Threading

Comment threading is the ability to reply to a reply. A good example is Reddit, where you can have a whole conversation between people in the comments section. YouTube supports threaded comments up to three levels of replies, meaning level 1 top-level comment, level 2 reply to top-level comment, and level 3 reply to reply.

There is a convention on YouTube that if you want to reply to someone else's reply, you write their user name **first** in your reply. This program takes advantage of this convention to build threaded comments for all comments, regardless of YouTube limitation of three levels of replies. This is enabled by default. You can disable comment threading with option `--disable-threading`. If disabled, shows the comments the way they appear on YouTube.

## Convert

Converts comments JSON file to text file or HTML file. Uses verb `convert`. Verb is required to specify.

```console
YouTubeCommentsToFile convert
    [--json-file PATH...]
    [--url URL]
    [--yt-dlp PATH]
    [--deno PATH]
    [PATH...]
```

### Convert Command Line Options

List of paths to comments JSON files or directories. If the specified path is a directory, the program will look for JSON files in that directory. The name of the output file is the same as the name of the JSON file. This option can be used multiple times. This option can have space-separated paths as its value. This option can be used interchangeably with JSON file paths placed at the end of the command line (`[PATH...]`). The `--json-file` option is useful when specifying paths in [configuration files](#configuration "Configuration").

```console
--json-file PATH...       Paths to comments JSON files or directories.
```

Since the video URL can't be extracted from the comments JSON file, by providing it, it makes the output file more robust by writing links to the video, to its comments and replies. URL is not required in order to write to output file.

```console
--url URL                 Video URL.
```

Path to yt-dlp program. If not set, search for yt-dlp in YouTubeCommentsToFile directory. If not found, check if yt-dlp is accessible in the current environment. It is not necessary to set `--yt-dlp` if yt-dlp is accessible, for example, yt-dlp directory is set in Windows `PATH` environment variable. yt-dlp is used to retrieve information about the video, therefore is not required to write to output file.

```console
--yt-dlp PATH             Path to yt-dlp.
```

Path to Deno program. If not set, search for Deno in YouTubeCommentsToFile directory. If not found, check if Deno is accessible in the current environment. If Deno is at the same location as yt-dlp, there is no need to specify a path to Deno. To download from YouTube, yt-dlp needs to solve JavaScript challenges presented by YouTube using an external JavaScript runtime. [Deno](https://deno.com "Deno") is a JavaScript runtime best suited for this and yt-dlp enables it by default.
- [Setup Guide](https://github.com/yt-dlp/yt-dlp/wiki/EJS "External JS Scripts Setup Guide").
- [Download Deno](https://github.com/denoland/deno "Deno"). For Windows, download `deno-x86_64-pc-windows-msvc.zip` from [Releases](https://github.com/denoland/deno/releases "Deno Releases").

```console
--deno PATH               Path to Deno.
```

### Convert Examples

1. Convert to text file:

```console
YouTubeCommentsToFile convert "C:\Path\To\Comments File.json"
```

2. Convert to HTML file:

```console
YouTubeCommentsToFile convert --to-html "C:\Path\To\Comments File.json"
```

3. Convert to more robust HTML file. By specifying the URL, yt-dlp will download video information - title, uploader, upload date, description:

```console
YouTubeCommentsToFile convert --json-file "C:\Path\To\Comments File.json" --url https://www.youtube.com/watch?v=dtCwxFTMMDg --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --to-html --show-comment-navigation-links --show-copy-links --uh
```

4. Highlight uploader, authors and texts. If a comment or reply matches any of the specified highlight items, the comment is highlighted:

```console
YouTubeCommentsToFile convert --uh --ah @Alice Bob --th "Alice and Bob" "are entangled" "C:\Path\To\Comments File.json"
```

5. Filter by uploader, authors and texts. If a comment or reply matches any of the specified filter items, it is shown. Otherwise, it is removed:

```console
YouTubeCommentsToFile convert --uf --af @Alice Bob --tf "Alice and Bob" "are entangled" "C:\Path\To\Comments File.json"
```

## Download

Downloads comments to text file or HTML file. Uses verb `download`. Verb is optional to specify.

```console
YouTubeCommentsToFile [download]
    [--url URL...]
    [--yt-dlp PATH]
    [--deno PATH]
    [--download-path PATH]
    [--max-comments NUMBER]
    [--max-parents NUMBER]
    [--max-replies NUMBER]
    [--max-replies-per-thread NUMBER]
    [--sort-new]
    [--sort-top]
    [--filename-sanitization]
    [--restrict-filenames]
    [--windows-filenames]
    [--trim-title LENGTH]
    [--only-download-comments]
    [--update-yt-dlp]
    [--yt-dlp-options OPTIONS]
    [URL...]
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

List of video URLs to download their comments. This option can be used multiple times. This option can have space-separated URLs as its value. This option can be used interchangeably with URLs placed at the end of the command line (`[URL...]`). The `--url` option is useful when specifying URLs in [configuration files](#configuration "Configuration").

```console
--url URL...                Video URLs to download their comments.
```

Path to yt-dlp program. If not set, search for yt-dlp in YouTubeCommentsToFile directory. If not found, check if yt-dlp is accessible in the current environment. It is not necessary to set `--yt-dlp` if yt-dlp is accessible, for example, yt-dlp directory is set in Windows `PATH` environment variable.

```console
--yt-dlp PATH               Path to yt-dlp.
```

Path to Deno program. If not set, search for Deno in YouTubeCommentsToFile directory. If not found, check if Deno is accessible in the current environment. If Deno is at the same location as yt-dlp, there is no need to specify a path to Deno. To download from YouTube, yt-dlp needs to solve JavaScript challenges presented by YouTube using an external JavaScript runtime. [Deno](https://deno.com "Deno") is a JavaScript runtime best suited for this and yt-dlp enables it by default. [Read more here](https://github.com/yt-dlp/yt-dlp/wiki/EJS "External JS Scripts Setup Guide").

```console
--deno PATH                 Path to Deno.
```

Path to where to download comments JSON file and convert it to text file or HTML file. If not set, download path is set to YouTubeCommentsToFile directory.

```console
--download-path PATH        Path to download and to convert comments.
```

These options limit the number of comments and replies. If an option is not set, it is unlimited (`all` in yt-dlp). If all options are not set, downloads everything.

```console
--max-comments NUMBER              Maximum number of top-level comments
                                   and replies to extract.
--max-parents NUMBER               Maximum number of top-level comments to extract.
--max-replies NUMBER               Maximum number of replies to extract.
--max-replies-per-thread NUMBER    Maximum number of replies per top-level comment
                                   to extract.
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
--sort-new                  Sort comments by new comments first.
--sort-top                  Sort comments by top comments first.
```

These options determine how the file name is processed.

```console
--filename-sanitization     Sanitize Windows reserved characters by removing them or
                            by replacing with comparable character or by replacing
                            with underscore. Doesn't replace with Unicode characters.
--restrict-filenames        Restrict filenames to only ASCII characters,
                            and avoid '&' and spaces in filenames.
--windows-filenames         Force filenames to be Windows-compatible by replacing
                            Windows reserved characters with lookalike Unicode characters.
--trim-title LENGTH         Limit the length of the video title in the filename
                            to the specified number of characters. Doesn't limit
                            the length of the video uploader.
```

Download the comments JSON file and don't write text and HTML files. This option can be useful if you want to experiment with the options that write to output file but don't want to repeatedly download the video comments. Download the comments once, `YouTubeCommentsToFile download --only-download-comments`, and convert them multiple times, `YouTubeCommentsToFile convert --json-file`.

```console
--only-download-comments    Whether to download comments JSON file and stop.
```

Update yt-dlp before downloading comments. Updates to `nightly` release channel. YouTube and yt-dlp developers are engaged in a cat and mouse chase, YouTube being the mouse and yt-dlp being the cat. YouTube is making frequent changes to the site in order to curve, limit and stop scraping tools such as yt-dlp. The developers of yt-dlp are quick to respond with changes to the program, but as a result of that, the users can no longer update yt-dlp once in a-very-long-time. If yt-dlp is not frequently updated (daily, weekly) to the newest version, it's more than likely it will stop working properly. Hence the update option. This option automates the updating process and takes the burden of remembering to update the program off the user's shoulders.

```console
--update-yt-dlp             Whether to update yt-dlp to the latest version.
```

Options for yt-dlp which are added to yt-dlp command line. This is for advanced yt-dlp features such as `--config-locations`, `--cookies`, `--cookies-from-browser`.

```console
--yt-dlp-options OPTIONS    Options added to yt-dlp command line.
```

Quick reminder, double quotes in double quotes need to be escaped with a backslash.

```console
--yt-dlp-options "--config-locations \"C:\A Path\With\A Space\yt-dlp.conf\" --cookies cookies.txt"
```

### Download Examples

1. Download to text file:

```console
YouTubeCommentsToFile download --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" https://www.youtube.com/watch?v=dtCwxFTMMDg
```

2. Download to HTML file:

```console
YouTubeCommentsToFile download --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --to-html https://www.youtube.com/watch?v=dtCwxFTMMDg
```

3. Download to more robust HTML file:

```console
YouTubeCommentsToFile download --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --to-html --show-comment-navigation-links --show-copy-links --uh https://www.youtube.com/watch?v=dtCwxFTMMDg
```

4. The `--yt-dlp` option can be omitted if yt-dlp is at the same directory as YouTubeCommentsToFile:

```console
YouTubeCommentsToFile download --download-path "C:\Path\To\Downloads Folder" https://www.youtube.com/watch?v=dtCwxFTMMDg
```

5. Download all comments, sort by top comments first:

```console
YouTubeCommentsToFile download --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --sort-top https://www.youtube.com/watch?v=dtCwxFTMMDg
```

6. Download up to 10,000 top-level comments and replies, at most 100 replies for each top-level comment, sort by new comments first (default):

```console
YouTubeCommentsToFile download --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --max-comments 10000 --max-replies-per-thread 100 https://www.youtube.com/watch?v=dtCwxFTMMDg
```

7. Download all top-level comments, up to 1000 replies, at most 10 replies for each top-level comment, sort by new comments first:

```console
YouTubeCommentsToFile download --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --max-replies 1000 --max-replies-per-thread 10 --sort-new https://www.youtube.com/watch?v=dtCwxFTMMDg
```

8. Download up to 1000 top-level comments, no replies, sort by top comments first:

```console
YouTubeCommentsToFile download --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --max-parents 1000 --max-replies 0 --sort-top https://www.youtube.com/watch?v=dtCwxFTMMDg
```

9. Download to text file, use configuration file for yt-dlp, use cookies file for restricted video:

```console
YouTubeCommentsToFile download --yt-dlp C:\Path\To\yt-dlp.exe --deno C:\Path\To\deno.exe --download-path "C:\Path\To\Downloads Folder" --yt-dlp-options "--config-locations \"C:\Path\To\yt-dlp.conf\" --cookies cookies.txt" https://www.youtube.com/watch?v=dtCwxFTMMDg
```

## Shared Options

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
--indent-size SIZE               The indentation size, in number of characters.
```

This option determines how many characters a single line of text is allowed. If a comment has a line of text that is longer than that, the line is split into several lines which adhere to that length limitation. Line length is between 80 and 320 characters. If this option is not set, line length for text file is 120 characters and line length for HTML file is 150 characters.

```console
--text-line-length LENGTH        The maximum length of a line of text,
                                 in number of characters.
```

Delete JSON file after comments were written successfully to the output file.

```console
--delete-json-file               Whether to delete the comments JSON file
                                 on successful completion.
```

## Uploader

Highlight uploader: Search for comments and replies written by the uploader of the video. If found, highlight the comment or reply.

Filter uploader: Search for comments and replies written by the uploader of the video. If found, keep the conversation (comment and its replies). Otherwise, remove the conversation, if it didn't match any other filter items.

```console
-u, --uh             Highlight uploader.
--uf                 Filter uploader.
--uhf                Highlight and filter uploader.
```

### Uploader Examples

1. Highlight all comments written by the uploader of the video:

```console
YouTubeCommentsToFile download --uh
```

2. Show only conversations that the uploader of the video participated in:

```console
YouTubeCommentsToFile download --uf
```

## Authors

Highlight authors: Search for comments and replies written by **any** of the specified authors. If found, highlight the comment or reply.

Filter authors: Search for comments and replies written by **any** of the specified authors. If found, keep the conversation (comment and its replies). Otherwise, remove the conversation, if it didn't match any other filter items.

Author name can be written with or without a leading @.

```console
--ah NAME...         Highlight authors.
--af NAME...         Filter authors.
--afi NAME...        Filter authors. Author search is case-insensitive.
--ahf NAME...        Highlight and filter authors.
--ahfi NAME...       Highlight and filter authors. Author search is case-insensitive.
-a, --ahi NAME...    Highlight authors. Author search is case-insensitive.
```

### Authors Examples

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

## Texts

Highlight texts: Search for **any** of the specified texts in the comment or reply. If found, highlight the comment or reply.

Filter texts: Search for **any** of the specified texts in the comment or reply. If found, keep the conversation (comment and its replies). Otherwise, remove the conversation, if it didn't match any other filter items.

```console
--th TEXT...         Highlight texts.
--tf TEXT...         Filter texts.
--tfi TEXT...        Filter texts. Text search is case-insensitive.
--thf TEXT...        Highlight and filter texts.
--thfi TEXT...       Highlight and filter texts. Text search is case-insensitive.
-t, --thi TEXT...    Highlight texts. Text search is case-insensitive.
```

### Texts Examples

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

## Encoding

The program works with UTF-8 as its default encoding and that should cover almost all languages. However, if the video page is not in English (the title, the description, majority of the comments) and downloading comments fails or comes out garbled, use the `--encoding-code-page` option to change the encoding. You need to find beforehand what is the code page of the non-English language.

```console
--encoding-code-page CODEPAGE    The code page of the video page encoding.
```

Or, use one of these predefined encodings.

```console
--utf8                           UTF-8. Code page 65001.
--arabic-iso                     Arabic (ISO-8859-6). Code page 28596.
                                 Arabic, Persian, Urdu.
--arabic-windows                 Arabic (Windows-1256). Code page 1256.
--baltic-iso-13                  Baltic (ISO-8859-13). Code page 28603.
                                 Lithuanian, Latvian, Estonian.
--baltic-iso-4                   Baltic (ISO-8859-4). Code page 28594.
                                 Estonian, Latvian, Lithuanian, Greenlandic, Sami.
--baltic-windows                 Baltic (Windows-1257). Code page 1257.
--central-european               Central European (Windows-1250). Code page 1250.
--chinese-simplified             Chinese Simplified (GB2312). Code page 936.
--chinese-traditional            Chinese Traditional (Big5). Code page 950.
--cyrillic-iso                   Cyrillic (ISO-8859-5). Code page 28595.
                                 Russian, Ukrainian, Bulgarian, Belarusian,
                                 Macedonian, Serbian.
--cyrillic-windows               Cyrillic (Windows-1251). Code page 1251.
--eastern-european               Eastern European (ISO-8859-2). Code page 28592.
                                 Czech, Hungarian, Polish, Romanian, Croatian,
                                 Slovak, Slovenian.
--greek-iso                      Greek (ISO-8859-7). Code page 28597.
--greek-windows                  Greek (Windows-1253). Code page 1253.
--hebrew-iso                     Hebrew (ISO-8859-8). Code page 28598.
--hebrew-windows                 Hebrew (Windows-1255). Code page 1255.
--japanese                       Japanese (Shift-JIS). Code page 932.
--korean                         Korean (Windows-949). Code page 949.
--south-european                 South European (ISO-8859-3). Code page 28593.
                                 Maltese, Esperanto, Turkish.
--thai                           Thai (TIS-620). Code page 874.
--turkish-iso                    Turkish (ISO-8859-9). Code page 28599.
--turkish-windows                Turkish (Windows-1254). Code page 1254.
--vietnamese                     Vietnamese (Windows-1258). Code page 1258.
--western-european-iso-1         Western European (ISO-8859-1). Code page 28591.
                                 English, French, Spanish, German, Italian, Portuguese.
--western-european-iso-15        Western European (ISO-8859-15). Code page 28605.
                                 English, French, German, Spanish, Finnish, Danish,
                                 Swedish, Italian, Dutch.
--western-european-windows       Western European (Windows-1252). Code page 1252.
```

## HTML Colors

These options set the colors of various HTML elements. A color value can be a known name (`Honeydew`), hex code (`#F0FFF0`; `F0FFF0`) or RGB (`240, 255, 240`; `240 255 240`).

```console
--text-color COLOR                             The text color of comments.
--background-color COLOR                       The background color of comments.
--author-color COLOR                           The text color of the author of the comment.
--replied-author-color COLOR                   The text color of an author, of other comment,
                                               appearing at the start of the comment.
--other-author-color COLOR                     The text color of an author, of other comment,
                                               appearing anywhere else but the start of the comment.
--small-elements-color COLOR                   The text color of small elements, such as
                                               pinned comment, comment time, likes.
--favorited-background-color COLOR             The background color of favorited by the
                                               uploader of the video.
--header-background-color COLOR                The background color of the header.
--highlight-uploader-text-color COLOR          The text color of highlighted comments
                                               written by the uploader of the video.
--highlight-uploader-background-color COLOR    The background color of highlighted comments
                                               written by the uploader of the video.
--highlight-uploader-border-color COLOR        The color of the border around highlighted comments
                                               written by the uploader of the video.
--highlight-author-text-color COLOR            The text color of highlighted comments
                                               written by any of the searched authors.
--highlight-author-background-color COLOR      The background color of highlighted comments
                                               written by any of the searched authors.
--highlight-author-border-color COLOR          The color of the border around highlighted comments
                                               written by any of the searched authors.
--highlight-text-text-color COLOR              The text color of highlighted comments
                                               containing any of the searched texts.
--highlight-text-background-color COLOR        The background color of highlighted comments
                                               containing any of the searched texts.
--highlight-text-border-color COLOR            The color of the border around highlighted comments
                                               containing any of the searched texts.
--link-color COLOR                             The text color of HTML links.
--copy-text-success-color COLOR                The text color of a success message after copying
                                               a comment to the clipboard.
--copy-text-failure-color COLOR                The text color of an error message after failing
                                               to copy a comment to the clipboard.
```

The colors of the HTML elements are determined by light theme (default theme) or dark theme (`--dark-theme`).
The following are the colors of these themes.

<table>
    <thead>
        <tr>
            <th/>
            <th colspan="2">Light Theme</th>
            <th colspan="2">Dark Theme</th>
        </tr>
        <tr>
            <th>HTML Element</th>
            <th>RGB</th>
            <th>Hex</th>
            <th>RGB</th>
            <th>Hex</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Text<br /><code>--text-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(255, 255, 255);'>15, 15, 15</td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(255, 255, 255);'>#0F0F0F</td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(15, 15, 15);'>241, 241, 241</td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(15, 15, 15);'>#F1F1F1</td>
        </tr>
        <tr>
            <td>Background<br /><code>--background-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(255, 255, 255);'>255, 255, 255</td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(255, 255, 255);'>#FFFFFF</td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(15, 15, 15);'>15, 15, 15</td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(15, 15, 15);'>#0F0F0F</td>
        </tr>
        <tr>
            <td>Author<br /><code>--author-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(255, 255, 255);'>15, 15, 15</td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(255, 255, 255);'>#0F0F0F</td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(15, 15, 15);'>241, 241, 241</td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(15, 15, 15);'>#F1F1F1</td>
        </tr>
        <tr>
            <td>Replied Author<br /><code>--replied-author-color</code></td>
            <td style='color: rgb(15, 128, 15); background-color: rgb(255, 255, 255);'>15, 128, 15</td>
            <td style='color: rgb(15, 128, 15); background-color: rgb(255, 255, 255);'>#0F800F</td>
            <td style='color: rgb(15, 128, 15); background-color: rgb(15, 15, 15);'>15, 128, 15</td>
            <td style='color: rgb(15, 128, 15); background-color: rgb(15, 15, 15);'>#0F800F</td>
        </tr>
        <tr>
            <td>Other Author<br /><code>--other-author-color</code></td>
            <td style='color: rgb(128, 15, 255); background-color: rgb(255, 255, 255);'>128, 15, 255</td>
            <td style='color: rgb(128, 15, 255); background-color: rgb(255, 255, 255);'>#800FFF</td>
            <td style='color: rgb(191, 15, 255); background-color: rgb(15, 15, 15);'>191, 15, 255</td>
            <td style='color: rgb(191, 15, 255); background-color: rgb(15, 15, 15);'>#BF0FFF</td>
        </tr>
        <tr>
            <td>Small Elements<br /><code>--small-elements-color</code></td>
            <td style='color: rgb(96, 96, 96); background-color: rgb(255, 255, 255);'>96, 96, 96</td>
            <td style='color: rgb(96, 96, 96); background-color: rgb(255, 255, 255);'>#606060</td>
            <td style='color: rgb(170, 170, 170); background-color: rgb(15, 15, 15);'>170, 170, 170</td>
            <td style='color: rgb(170, 170, 170); background-color: rgb(15, 15, 15);'>#AAAAAA</td>
        </tr>
        <tr>
            <td>Favorited Background<br /><code>--favorited-background-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(229, 235, 238);'>229, 235, 238</td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(229, 235, 238);'>#E5EBEE</td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(96, 96, 96);'>96, 96, 96</td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(96, 96, 96);'>#606060</td>
        </tr>
        <tr>
            <td>Header Background<br /><code>--header-background-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(242, 242, 242);'>242, 242, 242</td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(242, 242, 242);'>#F2F2F2</td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(39, 39, 39);'>39, 39, 39</td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(39, 39, 39);'>#272727</td>
        </tr>
        <tr>
            <td>Highlight Uploader: Text<br /><code>--highlight-uploader-text-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(255, 250, 238);'><span style='border: 1px solid rgb(183, 136, 0); padding: 10px;'>15, 15, 15</span></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(255, 250, 238);'><span style='border: 1px solid rgb(183, 136, 0); padding: 10px;'>#0F0F0F</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(140, 33, 23);'><span style='border: 1px solid rgb(252, 225, 150); padding: 10px;'>241, 241, 241</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(140, 33, 23);'><span style='border: 1px solid rgb(252, 225, 150); padding: 10px;'>#F1F1F1</span></td>
        </tr>
        <tr>
            <td>Highlight Uploader: Background<br /><code>--highlight-uploader-background-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(255, 250, 238);'><span style='border: 1px solid rgb(183, 136, 0); padding: 10px;'>255, 250, 238</span></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(255, 250, 238);'><span style='border: 1px solid rgb(183, 136, 0); padding: 10px;'>#FFFAEE</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(140, 33, 23);'><span style='border: 1px solid rgb(252, 225, 150); padding: 10px;'>140, 33, 23</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(140, 33, 23);'><span style='border: 1px solid rgb(252, 225, 150); padding: 10px;'>#8C2117</span></td>
        </tr>
        <tr>
            <td>Highlight Uploader: Border<br /><code>--highlight-uploader-border-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(255, 250, 238);'><span style='border: 1px solid rgb(183, 136, 0); padding: 10px;'>183, 136, 0</span></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(255, 250, 238);'><span style='border: 1px solid rgb(183, 136, 0); padding: 10px;'>#B78800</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(140, 33, 23);'><span style='border: 1px solid rgb(252, 225, 150); padding: 10px;'>252, 225, 150</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(140, 33, 23);'><span style='border: 1px solid rgb(252, 225, 150); padding: 10px;'>#FCE196</span></td>
        </tr>
        <tr>
            <td>Highlight Author: Text<br /><code>--highlight-author-text-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(252, 241, 237);'><span style='border: 1px solid rgb(255, 67, 5); padding: 10px;'>15, 15, 15</span></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(252, 241, 237);'><span style='border: 1px solid rgb(255, 67, 5); padding: 10px;'>#0F0F0F</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(137, 57, 16);'><span style='border: 1px solid rgb(255, 165, 0); padding: 10px;'>241, 241, 241</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(137, 57, 16);'><span style='border: 1px solid rgb(255, 165, 0); padding: 10px;'>#F1F1F1</span></td>
        </tr>
        <tr>
            <td>Highlight Author: Background<br /><code>--highlight-author-background-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(252, 241, 237);'><span style='border: 1px solid rgb(255, 67, 5); padding: 10px;'>252, 241, 237</span></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(252, 241, 237);'><span style='border: 1px solid rgb(255, 67, 5); padding: 10px;'>#FCF1ED</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(137, 57, 16);'><span style='border: 1px solid rgb(255, 165, 0); padding: 10px;'>137, 57, 16</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(137, 57, 16);'><span style='border: 1px solid rgb(255, 165, 0); padding: 10px;'>#893910</span></td>
        </tr>
        <tr>
            <td>Highlight Author: Border<br /><code>--highlight-author-border-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(252, 241, 237);'><span style='border: 1px solid rgb(255, 67, 5); padding: 10px;'>255, 67, 5</span></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(252, 241, 237);'><span style='border: 1px solid rgb(255, 67, 5); padding: 10px;'>#FF4305</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(137, 57, 16);'><span style='border: 1px solid rgb(255, 165, 0); padding: 10px;'>255, 165, 0</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(137, 57, 16);'><span style='border: 1px solid rgb(255, 165, 0); padding: 10px;'>#FFA500</span></td>
        </tr>
        <tr>
            <td>Highlight Text: Text<br /><code>--highlight-text-text-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(231, 239, 252);'><span style='border: 1px solid rgb(34, 110, 233); padding: 10px;'>15, 15, 15</span></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(231, 239, 252);'><span style='border: 1px solid rgb(34, 110, 233); padding: 10px;'>#0F0F0F</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(13, 57, 132);'><span style='border: 1px solid rgb(231, 239, 252); padding: 10px;'>241, 241, 241</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(13, 57, 132);'><span style='border: 1px solid rgb(231, 239, 252); padding: 10px;'>#F1F1F1</span></td>
        </tr>
        <tr>
            <td>Highlight Text: Background<br /><code>--highlight-text-background-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(231, 239, 252);'><span style='border: 1px solid rgb(34, 110, 233); padding: 10px;'>231, 239, 252</span></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(231, 239, 252);'><span style='border: 1px solid rgb(34, 110, 233); padding: 10px;'>#E7EFFC</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(13, 57, 132);'><span style='border: 1px solid rgb(231, 239, 252); padding: 10px;'>13, 57, 132</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(13, 57, 132);'><span style='border: 1px solid rgb(231, 239, 252); padding: 10px;'>#0D3984</span></td>
        </tr>
        <tr>
            <td>Highlight Text: Border<br /><code>--highlight-text-border-color</code></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(231, 239, 252);'><span style='border: 1px solid rgb(34, 110, 233); padding: 10px;'>34, 110, 233</span></td>
            <td style='color: rgb(15, 15, 15); background-color: rgb(231, 239, 252);'><span style='border: 1px solid rgb(34, 110, 233); padding: 10px;'>#226EE9</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(13, 57, 132);'><span style='border: 1px solid rgb(231, 239, 252); padding: 10px;'>231, 239, 252</span></td>
            <td style='color: rgb(241, 241, 241); background-color: rgb(13, 57, 132);'><span style='border: 1px solid rgb(231, 239, 252); padding: 10px;'>#E7EFFC</span></td>
        </tr>
        <tr>
            <td>Link<br /><code>--link-color</code></td>
            <td style='color: rgb(0, 0, 238); background-color: rgb(255, 255, 255);'>0, 0, 238</td>
            <td style='color: rgb(0, 0, 238); background-color: rgb(255, 255, 255);'>#0000EE</td>
            <td style='color: rgb(62, 166, 255); background-color: rgb(15, 15, 15);'>62, 166, 255</td>
            <td style='color: rgb(62, 166, 255); background-color: rgb(15, 15, 15);'>#3EA6FF</td>
        </tr>
        <tr>
            <td>Copy Text: Success<br /><code>--copy-text-success-color</code></td>
            <td style='color: rgb(15, 170, 15); background-color: rgb(255, 255, 255);'>15, 170, 15</td>
            <td style='color: rgb(15, 170, 15); background-color: rgb(255, 255, 255);'>#0FAA0F</td>
            <td style='color: rgb(15, 170, 15); background-color: rgb(15, 15, 15);'>15, 170, 15</td>
            <td style='color: rgb(15, 170, 15); background-color: rgb(15, 15, 15);'>#0FAA0F</td>
        </tr>
        <tr>
            <td>Copy Text: Failure<br /><code>--copy-text-failure-color</code></td>
            <td style='color: rgb(255, 15, 15); background-color: rgb(255, 255, 255);'>255, 15, 15</td>
            <td style='color: rgb(255, 15, 15); background-color: rgb(255, 255, 255);'>#FF0F0F</td>
            <td style='color: rgb(213, 15, 15); background-color: rgb(15, 15, 15);'>213, 15, 15</td>
            <td style='color: rgb(213, 15, 15); background-color: rgb(15, 15, 15);'>#D50F0F</td>
        </tr>
    </tbody>
</table>

## Configuration

Configuration files allow to set options for the program without having to specify them at the command line every time. A configuration file is a text file with a list of command line options.

```console
--config-locations PATH...    Paths to configuration files or to directories containing
                              configuration files. Can be used multiple times and inside
                              other configuration files.
```

- The option `--config-locations` can be used multiple times at the command line.
- A relative path, specified at the command line, is relative to the directory from which the program is running from.
- The option `--config-locations` can be used multiple times in a configuration file.
- A relative path, specified in a configuration file, is relative to the directory where the configuration file is at.
- If the path is a directory and not a file, the program looks for `youtube-comments-to-file-convert.conf`<sup>\*</sup>, `youtube-comments-to-file-download.conf`<sup>\*\*</sup>, `youtube-comments-to-file.conf` in the specified directory.
- Options passed at the command line always override the same options set within configuration files.
- Configuration file doesn't support verbs `convert` and `download`. The verb needs to be passed at the command line.

### Configuration File

- One option per line.
- No whitespace after `-` or `--`.
- Comment starts with #.
- Values with a space need to be between quotes, just like in the command line.
- Configuration files are decoded first with UTF-8 encoding and fallback to the current system locale encoding.

### Environment Variables

Environment variables can be specified in options that set a path to a file or to a directory:\
`--config-locations`, `--json-file`, `--download-path`, `--yt-dlp`, `--deno`.

An environment variable can be specified in one of three ways, regardless if the OS is Windows or Linux:\
`${VARIABLE}`, `$VARIABLE`, `%VARIABLE%`.

On Windows, `~` is mapped to `$HOME`. If `$HOME` doesn't exist, it falls back to `%USERPROFILE%`. If `%USERPROFILE%` doesn't exist, it falls back to `%HOMEDRIVE%%HOMEPATH%`.

On Windows, `%APPDATA%` usually located at `C:\Users\<username>\AppData\Roaming` and `%USERPROFILE%` usually located at `C:\Users\<username>`.

On Linux, if `$XDG_CONFIG_HOME` is not set, it falls back to `~/.config`. `$XDG_CACHE_HOME` falls back to `~/.cache`.

#### Environment Variables Examples

1. Two configuration files, one for `convert` operation and one for `download` operation, are located at the user's settings directory, `C:\Users\<username>\AppData\Roaming`, and are directly specified with `--config-locations` at the command line.

```console
YouTubeCommentsToFile convert --config-locations "%APPDATA%\convert.conf"
YouTubeCommentsToFile download --config-locations "%APPDATA%\download.conf"
```

2. The directory for downloaded comments is specified to be `YouTube Comments` directory under the user's profile directory. The full path is `C:\Users\<username>\YouTube Comments`.

```console
YouTubeCommentsToFile download --download-path "%USERPROFILE%\YouTube Comments"
```

3. You don't have to use the built-in environment variables. You can define custom ones and reference them.

- On Windows, open the **Environment Variables** window. The top half of the window is the user variables. The bottom half of the window is the system variables.
- In the user variables, add an new entry.\
Variable name: `YouTube Comments`\
Variable value: `C:\Users\<username>\YouTube Comments`
- Restart the computer. The environment variables are loaded when the computer starts.
- Use the environment variable `YouTube Comments` to specify the directory for downloaded comments.

```console
YouTubeCommentsToFile download --download-path "%YouTube Comments%"
```

### Configuration Stack

The program loads configuration files from multiple locations in the following order, from high priority to low priority. Options from high priority configuration take precedence over options from low priority configuration.

1. **Main Configuration**: Configuration files specified at the command line with `--config-locations`.
2. **Portable Configuration**: Configuration files in the same directory as the YouTubeCommentsToFile executable.
    - `youtube-comments-to-file-convert.conf`<sup>\*</sup>
    - `youtube-comments-to-file-download.conf`<sup>\*\*</sup>
    - `youtube-comments-to-file.conf`
3. **Home Configuration**: Configuration files at the directory that the program operates on. For `convert` operation, the directory of the JSON file, specified with `--json-file`. For `download` operation, the directory to download comments to, specified with `--download-path`.
    - `youtube-comments-to-file-convert.conf`<sup>\*</sup>
    - `youtube-comments-to-file-download.conf`<sup>\*\*</sup>
    - `youtube-comments-to-file.conf`
4. **User Configuration**:
    - Windows:
        - `%APPDATA%\youtube-comments-to-file-convert.conf`<sup>\*</sup>
        - `%APPDATA%\youtube-comments-to-file-download.conf`<sup>\*\*</sup>
        - `%APPDATA%\youtube-comments-to-file.conf`
        - `%APPDATA%\YouTubeCommentsToFile\convert`<sup>\*</sup>
        - `%APPDATA%\YouTubeCommentsToFile\download`<sup>\*\*</sup>
        - `%APPDATA%\YouTubeCommentsToFile\config`<sup>\*\*\*</sup>
        - `%APPDATA%\YouTubeCommentsToFile\convert.txt`<sup>\*</sup>
        - `%APPDATA%\YouTubeCommentsToFile\download.txt`<sup>\*\*</sup>
        - `%APPDATA%\YouTubeCommentsToFile\config.txt`
        - `%USERPROFILE%\youtube-comments-to-file-convert.conf`<sup>\*</sup>
        - `%USERPROFILE%\youtube-comments-to-file-download.conf`<sup>\*\*</sup>
        - `%USERPROFILE%\youtube-comments-to-file.conf`
        - `%USERPROFILE%\youtube-comments-to-file-convert.conf.txt`<sup>\*</sup>
        - `%USERPROFILE%\youtube-comments-to-file-download.conf.txt`<sup>\*\*</sup>
        - `%USERPROFILE%\youtube-comments-to-file.conf.txt`
        - `%USERPROFILE%\.youtubecommentstofile\convert`<sup>\*</sup>
        - `%USERPROFILE%\.youtubecommentstofile\download`<sup>\*\*</sup>
        - `%USERPROFILE%\.youtubecommentstofile\config`<sup>\*\*\*</sup>
        - `%USERPROFILE%\.youtubecommentstofile\convert.txt`<sup>\*</sup>
        - `%USERPROFILE%\.youtubecommentstofile\download.txt`<sup>\*\*</sup>
        - `%USERPROFILE%\.youtubecommentstofile\config.txt`
    - Linux:
        - `$XDG_CONFIG_HOME/youtube-comments-to-file-convert.conf`<sup>\*</sup>
        - `$XDG_CONFIG_HOME/youtube-comments-to-file-download.conf`<sup>\*\*</sup>
        - `$XDG_CONFIG_HOME/youtube-comments-to-file.conf`
        - `$XDG_CONFIG_HOME/youtube-comments-to-file/convert`<sup>\*</sup>
        - `$XDG_CONFIG_HOME/youtube-comments-to-file/download`<sup>\*\*</sup>
        - `$XDG_CONFIG_HOME/youtube-comments-to-file/config`<sup>\*\*\*</sup>
        - `$XDG_CONFIG_HOME/youtube-comments-to-file/convert.txt`<sup>\*</sup>
        - `$XDG_CONFIG_HOME/youtube-comments-to-file/download.txt`<sup>\*\*</sup>
        - `$XDG_CONFIG_HOME/youtube-comments-to-file/config.txt`
        - `~/youtube-comments-to-file-convert.conf`<sup>\*</sup>
        - `~/youtube-comments-to-file-download.conf`<sup>\*\*</sup>
        - `~/youtube-comments-to-file.conf`
        - `~/youtube-comments-to-file-convert.conf.txt`<sup>\*</sup>
        - `~/youtube-comments-to-file-download.conf.txt`<sup>\*\*</sup>
        - `~/youtube-comments-to-file.conf.txt`
        - `~/.youtube-comments-to-file/convert`<sup>\*</sup>
        - `~/.youtube-comments-to-file/download`<sup>\*\*</sup>
        - `~/.youtube-comments-to-file/config`<sup>\*\*\*</sup>
        - `~/.youtube-comments-to-file/convert.txt`<sup>\*</sup>
        - `~/.youtube-comments-to-file/download.txt`<sup>\*\*</sup>
        - `~/.youtube-comments-to-file/config.txt`
5. **System Configuration**: Linux only.
    - `/etc/youtube-comments-to-file-convert.conf`<sup>\*</sup>
    - `/etc/youtube-comments-to-file-download.conf`<sup>\*\*</sup>
    - `/etc/youtube-comments-to-file.conf`
    - `/etc/youtube-comments-to-file/convert`<sup>\*</sup>
    - `/etc/youtube-comments-to-file/download`<sup>\*\*</sup>
    - `/etc/youtube-comments-to-file/config`<sup>\*\*\*</sup>
    - `/etc/youtube-comments-to-file/convert.txt`<sup>\*</sup>
    - `/etc/youtube-comments-to-file/download.txt`<sup>\*\*</sup>
    - `/etc/youtube-comments-to-file/config.txt`

<sup>\*</sup> Only for `convert` operation.\
<sup>\**</sup> Only for `download` operation.\
<sup>\*\*\*</sup> The name of the file is `config` without a file extension.

### Configuration Control

This options ignores the current config location but continues loading the rest of the configuration stack. If this option is specified in the command line, all the `--config-locations`, in the command line, are not loaded. If this option is specified in a configuration file, the file is not loaded.

```console
--ignore-config        Don't load the config location and continue loading
                       the rest of the configuration stack.
```

This option stops the loading of the configuration stack from the point it appears. If this option is specified in the command line, the whole configuration stack is not loaded, including all the `--config-locations` in the command line. If this option is specified in a configuration file, the file is not loaded and the loading of the rest of the configuration stack is stopped.

```console
--stop-config          Don't load the config location and stop loading
                       the rest of the configuration stack.
```

## Verbosity

These options determine what and how much is printed out.

```console
-v, --verbose          Print debugging information.
-q, --quiet            Don't print anything.
                       If used with '--verbose', print to standard error (stderr).
--yt-dlp-quiet         Don't print output from yt-dlp.
--no-write-comments    Don't download or write comments.
```

## General

```console
--sleep-interval SECONDS        Number of seconds to sleep between converting
                                multiple JSON files or downloading from multiple URLs.
--min-sleep-interval SECONDS    Minimum number of seconds to sleep between converting
                                multiple JSON files or downloading from multiple URLs.
--max-sleep-interval SECONDS    Maximum number of seconds to sleep between converting
                                multiple JSON files or downloading from multiple URLs.
--beep                          Make a beep sound when finished.
                                If used with '--quiet', make no sound.
--beep-on-errors                Make a beep sound when finished with errors.
                                If used with '--quiet', make no sound.
--pause-on-errors               Prevent the console from closing until a key is pressed
                                when finished with errors. If used with '--quiet',
                                this option is disabled.
-h, --help                      Display help screen.
--version                       Display version information.
```

## Download Tutorials

These tutorials show how to use YouTubeCommentsToFile with more than just the command line.

### Preparation

1. Download YouTubeCommentsToFile from [Releases](https://github.com/yuvalsol/YouTubeCommentsToFile/releases "YouTubeCommentsToFile Releases").
2. Download and install [yt-dlp](https://github.com/yt-dlp/yt-dlp#installation "yt-dlp Installation").
3. Download [Deno](https://github.com/denoland/deno "Deno"). For Windows, download `deno-x86_64-pc-windows-msvc.zip` from [Deno Releases](https://github.com/denoland/deno/releases "Deno Releases").
4. Optional: Place YouTubeCommentsToFile, yt-dlp and Deno in the same directory. Make sure that directory **doesn't** need administrative rights (For example, on Windows, **not** under `C:\Program Files`) because you want to update yt-dlp with no prompts.

### Download With Script

1. Create this script [`download-comments.cmd`](./Solution%20Items/download-comments.cmd "download-comments.cmd"). This script downloads comments to HTML file and the comments are sorted by top comments first.

```batch
@echo off
set "url=%~1"
call "C:\Path\To\YouTubeCommentsToFile.exe" --url "%url%" --download-path "C:\Path\To\Downloads Folder" --max-comments 100000 --max-replies-per-thread 100 --sort-top --to-html --show-comment-navigation-links --show-copy-links --uh --trim-title 150 --delete-json-file --beep-on-errors --yt-dlp "C:\Path\To\yt-dlp.exe" --update-yt-dlp --deno "C:\Path\To\deno.exe"
exit /b %ERRORLEVEL%
```

You can omit the full path to the YouTubeCommentsToFile executable if the script is in the same directory as YouTubeCommentsToFile. You can omit `--yt-dlp` and `--deno` if the executables are in the same directory as the YouTubeCommentsToFile executable.

- `--url "URL"` Video URL.
- `--download-path "C:\Path\To\Downloads Folder"` Download to the specified directory.
- `--max-comments 100000 --max-replies-per-thread 100` At most 100,000 comments and replies, at most 100 replies for each top-level comment.
- `--sort-top` Sort by top comments first.
- `--to-html` Save comments to HTML output file.
- `--show-comment-navigation-links` Show next comment and previous comment navigation links.
- `--show-copy-links` Show copy text links.
- `--uh` Highlight uploader. Comments and replies, written by the uploader of the video, will be highlighted.
- `--trim-title 150` Limit the title of the video to at most 150 characters.
- `--delete-json-file` Delete the comments JSON file after successfully writing to the HTML output file.
- `--beep-on-errors` Make a beep sound when finished with errors.
- `--yt-dlp "C:\Path\To\yt-dlp.exe"` Path to yt-dlp.
- `--update-yt-dlp` Update yt-dlp before using it.
- `--deno "C:\Path\To\deno.exe"` Path to Deno.

2. Open Command Line/Terminal/shell in the same directory where YouTubeCommentsToFile is at or change the path to it.
3. Download comments with this command. Don't forget to change the URL below.

```console
download-comments "https://www.youtube.com/watch?v=dtCwxFTMMDg"
```

4. Repeat steps 2 and 3.

### Download From Browser

This tutorial shows how to download comments directly from the browser without copying and pasting the video URL somewhere else.

This download method uses **External Application Launcher** browser extension. This browser extension exposes custom arguments to external programs. This extension will pass the URL from the browser to the YouTubeCommentsToFile executable. [Read more](https://webextension.org/listing/external-application-button.html "External Application Button") on the extension website. The extension is applicable on [Chrome](https://chromewebstore.google.com/detail/external-application-laun/bifmfjgpgndemajpeeoiopbeilbaifdo "External Application Launcher") and other Chromium-based browsers, [Firefox](https://addons.mozilla.org/en-US/firefox/addon/external-application/ "External Application Button (WebExtension)") and [Edge](https://microsoftedge.microsoft.com/addons/detail/external-application-laun/icfhhpfimihpdgglmdlnmpaadfeaacbk "External Application Launcher").

1. Install External Application Launcher extension on your browser.
2. Create configuration file [`youtube-comments-to-file-download.conf`](./Solution%20Items/youtube-comments-to-file-download.conf "youtube-comments-to-file-download.conf") in the same directory as the YouTubeCommentsToFile executable. You can create this configuration file in other places. Read more about it in [Configuration Stack](#configuration-stack "Configuration Stack").

```ini
# Download to the specified directory.
--download-path "C:\Path\To\Downloads Folder"

# At most 100,000 comments and replies.
--max-comments 100000

# At most 100 replies for each top-level comment.
--max-replies-per-thread 100

# Sort by top comments first.
--sort-top

# Save comments to HTML output file.
--to-html

# Show next comment and previous comment navigation links.
--show-comment-navigation-links

# Show copy text links.
--show-copy-links

# Highlight uploader.
# Comments and replies, written by the uploader of the video, will be highlighted.
--uh

# Limit the title of the video to at most 150 characters.
--trim-title 150

# Delete the comments JSON file after successfully writing to the HTML output file.
--delete-json-file

# Make a beep sound when finished with errors.
--beep-on-errors

# Path to yt-dlp.
--yt-dlp "C:\Path\To\yt-dlp.exe"

# Update yt-dlp before using it.
--update-yt-dlp

# Path to Deno.
--deno "C:\Path\To\deno.exe"
```

3. Open the extension options page through your browser, fill in the following text and check boxes and press the "Add Application" button when done.
- **Display Name**: Download Comments
- **Executable Name**: C:\Windows\System32\cmd.exe
- **Arguments**: /c start /d "C:\Path\To\YouTubeCommentsToFile Directory" YouTubeCommentsToFile.exe --url \[HREF\]
- **Toolbar Button**: Page Context, Link Context, Video Context
- **Page Pattern**: \*://www.youtube.com/\*, \*://www.youtube-nocookie.com/\*, \*://music.youtube.com/\*, \*://www.bilibili.com/\*, \*://www.bilibili.tv/\*

If the configuration file is not part of the [Configuration Stack](#configuration-stack "Configuration Stack"), different name or different location, specify the path to it with `--config-locations`.

- **Arguments**: /c start /d "C:\Path\To\YouTubeCommentsToFile Directory" YouTubeCommentsToFile.exe --config-locations "C:\Path\To\configuration-file.conf" --url \[HREF\]

4. Go to YouTube and right-click on a link to a video in the YouTube main page or right-click on the video page itself. The button, to download comments, will be available in the context menu.

![External Application Launcher](./Solution%20Items/Images/ExternalApplicationLauncher.gif "External Application Launcher")

Side note, you can similarly use these steps to configure downloading the video itself with yt-dlp.
- **Display Name**: Download Video
- **Arguments**: /c start /d "C:\Path\To\yt-dlp Directory" yt-dlp.exe --config-location "C:\Path\To\yt-dlp-configuration-file.conf" \[HREF\]

## Acknowledgments

[yt-dlp](https://github.com/yt-dlp/yt-dlp "yt-dlp")\
[Comment icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/comment "comment icons")
