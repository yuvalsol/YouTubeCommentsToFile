@echo off
set "url=%~1"
call YouTubeCommentsToFile download --url "%url%" --max-comments 100000 --max-replies-per-thread 100 --sort-top --to-html --show-comment-navigation-links --show-copy-links --uh --trim-title 150 --delete-json-file --update-yt-dlp
exit /b %ERRORLEVEL%
