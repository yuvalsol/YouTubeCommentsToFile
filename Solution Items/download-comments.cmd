@echo off
set "url=%~1"
call "C:\Path\To\YouTubeCommentsToFile.exe" --url "%url%" --download-path "C:\Path\To\Downloads Folder" --max-comments 100000 --max-replies-per-thread 100 --sort-top --to-html --show-comment-navigation-links --show-copy-links --uh --trim-title 150 --delete-json-file --beep-errors --yt-dlp "C:\Path\To\yt-dlp.exe" --update-yt-dlp --deno "C:\Path\To\deno.exe"
exit /b %ERRORLEVEL%
