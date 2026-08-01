window.NavigationGuard = {

    isDirty: false,

    initialize: function () {

        if (window.NavigationGuard.initialized)
            return;

        window.NavigationGuard.initialized = true;

        document.body.addEventListener(
            "input",
            function (e) {
                console.log("INPUT EVENT", e.target);

                window.NavigationGuard.isDirty = true;
            },
            true);

        document.body.addEventListener(
            "change",
            function (e) {
                console.log("CHANGE EVENT", e.target);

                window.NavigationGuard.isDirty = true;
            },
            true);
    },

    hasChanges: function () {
        console.log("DIRTY =", window.NavigationGuard.isDirty);
        return window.NavigationGuard.isDirty;
    },

    clear: function () {
        window.NavigationGuard.isDirty = false;
    }
};





window.ClannGuard = {

    spawnParticles: function (containerId) {
        var container = document.getElementById(containerId);
        if (!container) return;

        // Inject @keyframes once into <head>
        if (!document.getElementById('clann-particle-keyframes')) {
            var style = document.createElement('style');
            style.id = 'clann-particle-keyframes';
            style.textContent =
                '@keyframes clann-float {' +
                '  0%   { transform: translateY(0) translateX(0);         opacity: 0;    }' +
                '  8%   { opacity: 1; }' +
                '  88%  { opacity: 0.25; }' +
                '  100% { transform: translateY(-320px) translateX(18px); opacity: 0;    }' +
                '}';
            document.head.appendChild(style);
        }

        for (var i = 0; i < 20; i++) {
            var p = document.createElement('span');
            var size = 1 + Math.random() * 2.5;

            p.style.position = 'absolute';
            p.style.borderRadius = '50%';
            p.style.background = 'var(--clann-particle-color, rgba(180,220,255,0.38))';
            p.style.pointerEvents = 'none';
            p.style.width = size + 'px';
            p.style.height = size + 'px';
            p.style.left = (Math.random() * 100) + '%';
            p.style.bottom = '-4px';
            p.style.opacity = (0.25 + Math.random() * 0.55).toString();
            p.style.animation = 'clann-float linear infinite';
            p.style.animationDuration = (7 + Math.random() * 11) + 's';
            p.style.animationDelay = (Math.random() * 9) + 's';

            container.appendChild(p);
        }
    }

};
