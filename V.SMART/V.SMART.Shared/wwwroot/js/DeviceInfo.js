window.deviceHelper = {

    getDeviceId: function () {

        let id = localStorage.getItem("VSMART_DEVICE_ID");

        if (!id) {
            id = crypto.randomUUID();
            localStorage.setItem("VSMART_DEVICE_ID", id);
        }

        return id;
    },

    isMobile: function () {
        return /Android|iPhone|iPad|iPod|Mobile/i.test(navigator.userAgent);
    },

    getPlatform: function () {
        return navigator.platform;
    },

    getUserAgent: function () {
        return navigator.userAgent;
    }
};