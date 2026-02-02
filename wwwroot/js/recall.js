// Recall page JavaScript
// This file can be expanded if you need edit functionality for recalls

async function editRecall(recallId, newDate, newStatus) {
    const response = await fetch('/Recall/EditRecall', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `recallId=${recallId}&recallDate=${newDate}&status=${newStatus}`
    });

    const result = await response.json();
    if (result.success) {
        location.reload();
    } else {
        alert('Failed to update recall');
    }
}