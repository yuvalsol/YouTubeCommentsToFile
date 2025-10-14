document.addEventListener('DOMContentLoaded', function() {
    const copyLinks = document.querySelectorAll('span.copy-text');
    for (const copyLink of copyLinks) {
        copyLink.addEventListener('click', function(e) {
            const lnk = e.target;
            const id = lnk.getAttribute('data-id');
            const lines = document.querySelectorAll('span.comment[data-id="' + id + '"],span.reply[data-id="' + id + '"]');
            const text = Array.from(lines).map(line => line.innerText || line.textContent).join('\n');

            if (navigator.clipboard && navigator.clipboard.writeText) {
                try {
                    navigator.clipboard.writeText(text);
                    setCopyResult(lnk, true);
                } catch(err) {
                    setCopyResult(lnk, false, err);
                }
            } else {
                const textArea = document.createElement('textarea');
                textArea.value = text;
                document.body.appendChild(textArea);
                textArea.select();
                try {
                    document.execCommand('copy');
                    setCopyResult(lnk, true);
                } catch(err) {
                    setCopyResult(lnk, false, err);
                } finally {
                    document.body.removeChild(textArea);
                }
            }

            return false;
        });
    }
});

function setCopyResult(lnk, succeeded, err) {
    lnk.innerText = (succeeded ? "Copied" : "Copy Failed");
    lnk.classList.add(succeeded ? "copy-text-success" : "copy-text-failure");

    setTimeout(function () {
        lnk.classList.remove(succeeded ? "copy-text-success" : "copy-text-failure");
        lnk.innerText = "Copy";
    }, 1500);

    if (err) {
        alert('Failed to copy to clipboard: ' + err.message);
    }
}