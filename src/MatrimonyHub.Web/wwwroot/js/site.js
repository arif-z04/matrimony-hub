// Matrimony Hub Client-side Interactive Utilities

document.addEventListener('DOMContentLoaded', function () {
    // 1. Auto-dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 6000);
    });

    // 2. Photo upload preview helper
    const fileInputs = document.querySelectorAll('input[type="file"][data-preview]');
    fileInputs.forEach(input => {
        input.addEventListener('change', function () {
            const targetId = this.getAttribute('data-preview');
            const targetImg = document.getElementById(targetId);
            if (targetImg && this.files && this.files[0]) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    targetImg.src = e.target.result;
                    targetImg.style.display = 'block';
                };
                reader.readAsDataURL(this.files[0]);
            }
        });
    });

    // 3. Dynamic AJAX Favorite toggle
    const favButtons = document.querySelectorAll('.btn-fav-ajax');
    favButtons.forEach(btn => {
        btn.addEventListener('click', async function (e) {
            e.preventDefault();
            const profileId = this.getAttribute('data-profile-id');
            if (!profileId) return;

            try {
                this.disabled = true;
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                const response = await fetch(`/api/favorites/${profileId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token || ''
                    }
                });

                if (response.status === 401) {
                    window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                    return;
                }

                const data = await response.json();
                if (data.succeeded) {
                    const icon = this.querySelector('i');
                    if (icon) {
                        if (icon.classList.contains('bi-heart-fill')) {
                            icon.classList.remove('bi-heart-fill', 'text-danger');
                            icon.classList.add('bi-heart');
                        } else {
                            icon.classList.remove('bi-heart');
                            icon.classList.add('bi-heart-fill', 'text-danger');
                        }
                    }
                }
            } catch (err) {
                console.error('Favorite toggle failed', err);
            } finally {
                this.disabled = false;
            }
        });
    });
});
