// Fixed Inventory JavaScript with proper anti-forgery token handling

function showAddItemModal() {
    document.getElementById('modalTitle').textContent = 'Add Item';
    document.getElementById('itemForm').reset();
    document.getElementById('itemId').value = '';
    document.getElementById('itemModal').style.display = 'block';
}

function closeItemModal() {
    document.getElementById('itemModal').style.display = 'none';
}

// FIXED: Helper function to get anti-forgery token from the hidden input field
function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenInput) {
        console.error('Anti-forgery token not found in page');
        return '';
    }
    return tokenInput.value;
}

async function editItem(id) {
    try {
        const response = await fetch(`/Inventory/GetItem?id=${id}`);

        // Check if response is ok before parsing JSON
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        // Check content type before parsing
        const contentType = response.headers.get('content-type');
        if (!contentType || !contentType.includes('application/json')) {
            throw new Error('Server did not return JSON response');
        }

        const result = await response.json();

        if (result.success === false) {
            alert('Failed to load item: ' + (result.message || 'Unknown error'));
            return;
        }

        // Access nested item property from result
        const item = result.item || result;

        document.getElementById('modalTitle').textContent = 'Edit Item';
        document.getElementById('itemId').value = item.itemId;
        document.getElementById('itemName').value = item.itemName;
        document.getElementById('category').value = item.category;
        document.getElementById('quantity').value = item.quantity;
        document.getElementById('unit').value = item.unit || '';
        document.getElementById('minimumStock').value = item.minimumStock;
        document.getElementById('supplier').value = item.supplier || '';
        document.getElementById('itemModal').style.display = 'block';
    } catch (error) {
        alert('Error loading item: ' + error.message);
        console.error('Error:', error);
    }
}

async function deleteItem(id) {
    if (!confirm('Are you sure you want to delete this item?')) return;

    try {
        const token = getAntiForgeryToken();
        if (!token) {
            alert('Security token not found. Please refresh the page and try again.');
            return;
        }

        // FIXED: Include token in form data instead of custom header
        const formData = new URLSearchParams();
        formData.append('id', id);
        formData.append('__RequestVerificationToken', token);

        const response = await fetch('/Inventory/DeleteItem', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: formData.toString()
        });

        // Check content type
        const contentType = response.headers.get('content-type');
        if (!contentType || !contentType.includes('application/json')) {
            throw new Error('Server did not return JSON response');
        }

        const result = await response.json();

        if (result.success) {
            alert('Item deleted successfully');
            location.reload();
        } else {
            alert('Failed to delete item: ' + (result.message || 'Unknown error'));
        }
    } catch (error) {
        alert('Error deleting item: ' + error.message);
        console.error('Error:', error);
    }
}

// FIXED: Improved form submission with comprehensive error handling
document.getElementById('itemForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const itemId = document.getElementById('itemId').value;
    const itemName = document.getElementById('itemName').value;
    const category = document.getElementById('category').value;
    const quantity = document.getElementById('quantity').value;
    const unit = document.getElementById('unit').value;
    const minimumStock = document.getElementById('minimumStock').value;
    const supplier = document.getElementById('supplier').value;

    // Validate required fields
    if (!itemName || !category || !quantity || !unit || !minimumStock || !supplier) {
        alert('Please fill in all required fields');
        return;
    }

    // Validate numeric fields
    if (isNaN(quantity) || quantity < 0) {
        alert('Quantity must be a non-negative number');
        return;
    }

    if (isNaN(minimumStock) || minimumStock < 0) {
        alert('Minimum stock must be a non-negative number');
        return;
    }

    // Get anti-forgery token
    const token = getAntiForgeryToken();
    if (!token) {
        alert('Security token not found. Please refresh the page and try again.');
        return;
    }

    // Determine action
    const action = itemId ? 'UpdateItem' : 'AddItem';

    // FIXED: Include anti-forgery token in form data
    const params = new URLSearchParams();
    params.append('__RequestVerificationToken', token);

    if (itemId) {
        params.append('itemId', itemId);
    }
    params.append('itemName', itemName);
    params.append('category', category);
    params.append('quantity', quantity);
    params.append('unit', unit);
    params.append('minimumStock', minimumStock);
    params.append('supplier', supplier);

    try {
        console.log('Sending request to:', `/Inventory/${action}`);
        console.log('Form data:', params.toString());

        const response = await fetch(`/Inventory/${action}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: params.toString()
        });

        console.log('Response status:', response.status);
        console.log('Response headers:', [...response.headers.entries()]);

        // FIXED: Check content type before attempting to parse JSON
        const contentType = response.headers.get('content-type');
        if (!contentType || !contentType.includes('application/json')) {
            // If the response is not JSON, it might be HTML (redirect, error page, etc.)
            const responseText = await response.text();
            console.error('Non-JSON response received:', responseText.substring(0, 500));
            throw new Error('Server did not return JSON response. Status: ' + response.status);
        }

        // Parse JSON response
        const result = await response.json();
        console.log('Response data:', result);

        // Check if the operation was successful
        if (result.success) {
            alert('Item saved successfully');
            closeItemModal();
            location.reload();
        } else {
            // Show error message from server
            alert('Failed to save item: ' + (result.message || 'Unknown error'));
        }
    } catch (error) {
        // Handle network errors and parsing errors
        console.error('Error details:', error);
        alert('Error saving item: ' + error.message);
    }
});

// Close modal when clicking outside
window.onclick = function (event) {
    const modal = document.getElementById('itemModal');
    if (event.target == modal) {
        modal.style.display = "none";
    }
}