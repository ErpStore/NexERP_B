let idleTimer;
let dotNetHelper;

window.idleTimer = {
    start: function (dotNetObj, timeoutInMinutes) {
        dotNetHelper = dotNetObj;

        function resetTimer() {
            clearTimeout(idleTimer);

            idleTimer = setTimeout(() => {
                dotNetHelper.invokeMethodAsync('LogoutUser');
            }, timeoutInMinutes * 60 * 1000);
        }

        document.onmousemove = resetTimer;
        document.onkeypress = resetTimer;
        document.onclick = resetTimer;
        document.onscroll = resetTimer;

        resetTimer();
    }
};