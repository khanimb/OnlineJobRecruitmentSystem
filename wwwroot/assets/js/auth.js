function getToken() {
    return localStorage.getItem('token');
}

function getUser() {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
}

function isLoggedIn() {
    return !!getToken();
}

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/pages/login.html';
}

function updateNavbar() {
    const user = getUser();
    if (!user) return;

    const navBtns = document.querySelector('.nav-btns');
    if (!navBtns) return;

    if (user.role === 'Employer') {
        navBtns.innerHTML = `
      <a href="/pages/employer-dashboard.html" class="btn-outline">Dashboard</a>
      <button class="btn-primary" onclick="logout()">Logout</button>
    `;
    } else if (user.role === 'JobSeeker') {
        navBtns.innerHTML = `
      <a href="/pages/jobseeker-dashboard.html" class="btn-outline">Dashboard</a>
      <button class="btn-primary" onclick="logout()">Logout</button>
    `;
    }
}

document.addEventListener('DOMContentLoaded', updateNavbar);