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
    window.location.href = '/assets/pages/login.html';
}

function updateNavbar() {
    const user = getUser();
    if (!user) return;

    const navBtns = $('.nav-btns');
    if (!navBtns.length) return;

    if (user.role === 'Employer') {
        navBtns.html(`
      <a href="/assets/pages/employerdashboard.html" class="btn-outline">Dashboard</a>
      <button class="btn-primary" onclick="logout()">Logout</button>
    `);
    } else if (user.role === 'JobSeeker') {
        navBtns.html(`
      <a href="/assets/pages/jobseekerdashboard.html" class="btn-outline">Dashboard</a>
      <button class="btn-primary" onclick="logout()">Logout</button>
    `);
    }
}

$(updateNavbar);