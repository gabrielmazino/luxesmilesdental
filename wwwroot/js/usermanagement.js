function showAddAccountModal() {
    document.getElementById('modalTitle').textContent = 'Add Account';
    document.getElementById('userForm').reset();
    document.getElementById('userId').value = '';
    document.getElementById('password').required = true;
    document.getElementById('passwordHint').style.display = 'none';
    document.getElementById('userModal').style.display = 'block';
}

function closeUserModal() {
    document.getElementById('userModal').style.display = 'none';
}

async function editUser(id) {
    try {
        const response = await fetch(`/UserManagement/GetUser?id=${id}`);
        const user = await response.json();

        document.getElementById('modalTitle').textContent = 'Edit Account';
        document.getElementById('userId').value = user.userId;
        document.getElementById('employeeName').value = user.employeeName;
        document.getElementById('username').value = user.username;
        document.getElementById('password').value = '';
        document.getElementById('password').required = false;
        document.getElementById('passwordHint').style.display = 'block';
        document.getElementById('role').value = user.role;
        document.getElementById('contactNumber').value = user.contactNumber || '';
        document.getElementById('userModal').style.display = 'block';
    } catch (error) {
        alert('Error loading user: ' + error.message);
        console.error('Error:', error);
    }
}

async function toggleActive(userId) {
    try {
        const response = await fetch('/UserManagement/ToggleActive', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `userId=${userId}`
        });

        const result = await response.json();

        if (result.success) {
            location.reload();
        } else {
            alert('Failed to toggle user status');
        }
    } catch (error) {
        alert('Error toggling user status: ' + error.message);
        console.error('Error:', error);
    }
}

document.getElementById('userForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const userId = document.getElementById('userId').value;
    const employeeName = document.getElementById('employeeName').value;
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const role = document.getElementById('role').value;
    const contactNumber = document.getElementById('contactNumber').value;

    // Validate required fields
    if (!employeeName || !username || !role) {
        alert('Please fill in all required fields');
        return;
    }

    // Check if password is required for new users
    if (!userId && !password) {
        alert('Password is required for new accounts');
        return;
    }

    const action = userId ? 'EditAccount' : 'AddAccount';

    const params = new URLSearchParams();
    if (userId) {
        params.append('userId', userId);
    }
    params.append('employeeName', employeeName);
    params.append('username', username);
    if (password) {
        params.append('password', password);
    }
    params.append('role', role);
    if (contactNumber) {
        params.append('contactNumber', contactNumber);
    }

    try {
        console.log('Sending request to:', `/UserManagement/${action}`);
        console.log('Form data:', params.toString());

        const response = await fetch(`/UserManagement/${action}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: params.toString()
        });

        const result = await response.json();
        console.log('Response:', result);

        if (result.success) {
            location.reload();
        } else {
            alert('Failed to save user: ' + (result.message || 'Unknown error'));
        }
    } catch (error) {
        alert('Error saving user: ' + error.message);
        console.error('Error:', error);
    }
});

window.onclick = function (event) {
    const modal = document.getElementById('userModal');
    if (event.target == modal) {
        modal.style.display = "none";
    }
}