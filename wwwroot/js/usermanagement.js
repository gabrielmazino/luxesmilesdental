// Helper function to get anti-forgery token
function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : '';
}

function showAddUserModal() {
    document.getElementById('modalTitle').textContent = 'Add User';
    document.getElementById('userId').value = '';
    document.getElementById('employeeName').value = '';
    document.getElementById('username').value = '';
    document.getElementById('password').value = '';
    document.getElementById('role').value = 'Dentist';
    document.getElementById('contactNumber').value = '';
    document.getElementById('passwordGroup').style.display = 'block';
    document.getElementById('password').required = true;
    document.getElementById('passwordHint').style.display = 'none';
    document.getElementById('userModal').style.display = 'block';
}

function closeUserModal() {
    document.getElementById('userModal').style.display = 'none';
}

async function editUser(id) {
    try {
        const response = await fetch('/UserManagement/GetUser?id=' + id);

        if (!response.ok) {
            throw new Error('HTTP error! status: ' + response.status);
        }

        const result = await response.json();

        if (result.success === false) {
            alert('Failed to load user: ' + (result.message || 'Unknown error'));
            return;
        }

        const user = result.user || result;

        // Support both PascalCase and camelCase responses
        const userId = user.UserId || user.userId;
        const empName = user.EmployeeName || user.employeeName;
        const uname = user.Username || user.username;
        const role = user.Role || user.role;
        const contact = user.ContactNumber || user.contactNumber || '';

        document.getElementById('modalTitle').textContent = 'Edit User';
        document.getElementById('userId').value = userId;
        document.getElementById('employeeName').value = empName;
        document.getElementById('username').value = uname;
        document.getElementById('role').value = role;
        document.getElementById('contactNumber').value = contact;
        // Show password field in edit too — leave blank to keep existing password
        document.getElementById('password').value = '';
        document.getElementById('password').required = false;
        document.getElementById('passwordGroup').style.display = 'block';
        document.getElementById('passwordHint').style.display = 'block';
        document.getElementById('userModal').style.display = 'block';

    } catch (error) {
        alert('Error loading user: ' + error.message);
        console.error('Error:', error);
    }
}

async function toggleUserStatus(id) {
    try {
        const token = getAntiForgeryToken();

        const response = await fetch('/UserManagement/ToggleStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token
            },
            body: 'id=' + id
        });

        if (!response.ok) {
            throw new Error('HTTP error! status: ' + response.status);
        }

        const result = await response.json();

        if (result.success) {
            location.reload();
        } else {
            alert('Failed to toggle user status: ' + (result.message || 'Unknown error'));
        }
    } catch (error) {
        alert('Error toggling user status: ' + error.message);
        console.error('Error:', error);
    }
}

function handleFormSubmit(e) {
    e.preventDefault();

    const userId = document.getElementById('userId').value;
    const employeeName = document.getElementById('employeeName').value.trim();
    const username = document.getElementById('username').value.trim();
    const role = document.getElementById('role').value;
    const contactNumber = document.getElementById('contactNumber').value.trim();
    const password = document.getElementById('password').value;

    if (!employeeName || !username || !role) {
        alert('Please fill in all required fields');
        return;
    }

    if (!userId && !password) {
        alert('Please enter a password for the new user');
        return;
    }

    const action = userId ? 'UpdateUser' : 'AddUser';
    const token = getAntiForgeryToken();

    const params = new URLSearchParams();
    if (userId) params.append('userId', userId);
    params.append('employeeName', employeeName);
    params.append('username', username);
    params.append('role', role);
    params.append('contactNumber', contactNumber);
    // Send password for new users (required), or for updates only if filled in
    if (!userId) {
        params.append('password', password);
    } else if (password) {
        params.append('password', password);
    }

    fetch('/UserManagement/' + action, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: params.toString()
    })
        .then(function (response) {
            if (!response.ok) throw new Error('HTTP error! status: ' + response.status);
            return response.json();
        })
        .then(function (result) {
            if (result.success) {
                alert('User saved successfully');
                location.reload();
            } else {
                alert('Failed to save user: ' + (result.message || 'Unknown error'));
            }
        })
        .catch(function (error) {
            alert('Error saving user: ' + error.message);
            console.error('Error:', error);
        });
}

// Attach form submit handler safely
var form = document.getElementById('userForm');
if (form) {
    form.addEventListener('submit', handleFormSubmit);
}

window.onclick = function (event) {
    var modal = document.getElementById('userModal');
    if (modal && event.target === modal) {
        modal.style.display = 'none';
    }
};