/* Your Global Scripts */

(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const externalLoginButtons = document.querySelectorAll('button[name="provider"]');
        if (!externalLoginButtons.length) return;

        const iconMap = {
            'GitHub': '<i class="fa fa-github me-2"></i>',
            'Gitee': '<i class="fas fa-gitlab me-2"></i>',
            'QQ': '<i class="fab fa-qq me-2"></i>',
            'Weixin': '<i class="fab fa-weixin me-2"></i>',
            'WorkWeixin': '<i class="fas fa-building me-2"></i>',
            'Bilibili': '<i class="fab fa-bilibili me-2"></i>',
            'Microsoft': '<i class="fab fa-microsoft me-2"></i>',
            'Google': '<i class="fab fa-google me-2"></i>'
        };



        const colorMap = {
            'GitHub': 'btn-dark',
            'Gitee': 'btn-secondary',
            'QQ': 'btn-info',
            'Weixin': 'btn-success',
            'WorkWeixin': 'btn-success',
            'Bilibili': 'btn-danger',
            'Microsoft': 'btn-dark',
            'Google': 'btn-light text-dark'
        };

        externalLoginButtons.forEach(button => {
            const provider = button.value;
            const displayName = button.textContent.trim();
            const iconHtml = iconMap[provider];
            const btnColor = colorMap[provider] || 'btn-primary';

            if (iconHtml) {
                button.innerHTML = iconHtml + displayName;
                button.className = `btn ${btnColor} m-1`;
            }
        });
    });
})();



