function showAddItemModal() {
    document.getElementById('modalTitle').textContent = 'Add Item';
    document.getElementById('itemForm').reset();
    document.getElementById('itemId').value = '';
    document.getElementById('itemModal').style.display = 'block';
}

function closeItemModal() {
    document.getElementById('itemModal').style.display = 'none';
}

async function editItem(id) {
    try {
        const response = await fetch(`/Inventory/GetItem?id=${id}`);
        const item = await response.json();

        if (item.success === false) {
            alert('Failed to load item: ' + (item.message || 'Unknown error'));
            return;
        }

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
        const response = await fetch('/Inventory/DeleteItem', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `id=${id}`
        });

        const result = await response.json();

        if (result.success) {
            location.reload();
        } else {
            alert('Failed to delete item: ' + (result.message || 'Unknown error'));
        }
    } catch (error) {
        alert('Error deleting item: ' + error.message);
        console.error('Error:', error);
    }
}

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

    const action = itemId ? 'EditItem' : 'AddItem';

    const params = new URLSearchParams();
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

        const result = await response.json();
        console.log('Response:', result);

        if (result.success) {
            location.reload();
        } else {
            alert('Failed to save item: ' + (result.message || 'Unknown error'));
        }
    } catch (error) {
        alert('Error saving item: ' + error.message);
        console.error('Error:', error);
    }
});

window.onclick = function (event) {
    const modal = document.getElementById('itemModal');
    if (event.target == modal) {
        modal.style.display = "none";
    }
}