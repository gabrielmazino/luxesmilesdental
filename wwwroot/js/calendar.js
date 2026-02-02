// ------------------- MODAL CONTROLS -------------------

function showNewAppointmentModal() {
    document.getElementById('appointmentModal').style.display = 'block';
    const form = document.getElementById('appointmentForm');
    if (form) form.reset();
}

function closeModal() {
    document.getElementById('appointmentModal').style.display = 'none';
}

function closeDayModal() {
    document.getElementById('dayAppointmentsModal').style.display = 'none';
}

function closeViewModal() {
    document.getElementById('viewAppointmentModal').style.display = 'none';
}

// ------------------- CREATE APPOINTMENT -------------------

async function createAppointment() {
    const patientName = document.getElementById('patientName').value;
    const dentistId = document.getElementById('dentistId').value;
    const appointmentDate = document.getElementById('appointmentDate').value;
    const appointmentTime = document.getElementById('appointmentTime').value;
    const notes = document.getElementById('appointmentNotes').value;

    if (!patientName || !dentistId || !appointmentDate) {
        alert('Please fill in all required fields');
        return;
    }

    const params = new URLSearchParams({
        patientName,
        dentistId,
        appointmentDate,
        appointmentTime: appointmentTime || '',
        notes: notes || ''
    });

    try {
        const response = await fetch('/Calendar/CreateAppointment', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: params.toString()
        });

        const result = await response.json();

        if (result.success) {
            alert('Appointment created successfully!');
            location.reload();
        } else {
            alert(result.message || 'Failed to create appointment');
        }
    } catch (error) {
        alert('Error creating appointment: ' + error.message);
        console.error('Error:', error);
    }
}

// ------------------- SHOW DAY APPOINTMENTS -------------------

async function showDayAppointments(date) {
    try {
        console.log('Fetching appointments for date:', date);

        const response = await fetch(`/Calendar/GetDayAppointments?date=${encodeURIComponent(date)}`);
        const result = await response.json();

        console.log('Response:', result);

        if (!result.success) {
            alert(result.message || 'Failed to load appointments');
            return;
        }

        const appointments = result.appointments;
        const dateObj = new Date(date + 'T00:00:00');
        const formattedDate = dateObj.toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });

        document.getElementById('dayModalTitle').textContent = `Appointments - ${formattedDate}`;

        const appointmentsList = document.getElementById('dayAppointmentsList');
        if (appointments.length === 0) {
            appointmentsList.innerHTML = '<p style="text-align: center; color: #666; padding: 20px;">No appointments for this day</p>';
            document.getElementById('dayAppointmentsModal').style.display = 'block';
            return;
        }

        let html = '<table class="data-table" style="margin-top: 20px;">';
        html += '<thead><tr><th>Time</th><th>Patient</th><th>Dentist</th><th>Status</th><th>Action</th></tr></thead>';
        html += '<tbody>';

        appointments.forEach(apt => {
            let statusClass = 'status-instock';
            if (apt.status === 'Cancelled') statusClass = 'status-overdue';
            else if (apt.status === 'Completed') statusClass = 'status-upcoming';

            html += `<tr>
                <td><strong>${apt.time}</strong></td>
                <td>${apt.patientName}</td>
                <td>${apt.dentistName}</td>
                <td><span class="status-badge ${statusClass}">${apt.status}</span></td>
                <td><button class="btn-primary" style="padding: 6px 12px; font-size: 13px;" onclick="viewAppointmentDetails(${apt.appointmentId})">View Details</button></td>
            </tr>`;
        });

        html += '</tbody></table>';
        appointmentsList.innerHTML = html;
        document.getElementById('dayAppointmentsModal').style.display = 'block';

    } catch (error) {
        alert('Error loading appointments: ' + error.message);
        console.error('Error:', error);
    }
}

// ------------------- VIEW APPOINTMENT DETAILS -------------------

async function viewAppointmentDetails(appointmentId) {
    try {
        const response = await fetch(`/Calendar/GetAppointmentDetails?id=${appointmentId}`);
        const result = await response.json();

        if (!result.success) {
            alert(result.message || 'Failed to load appointment details');
            return;
        }

        const apt = result.appointment; // ✅ FIX: access nested appointment object

        const detailsHtml = `
            <div style="padding: 20px;">
                <div class="form-group">
                    <label><strong>Patient Name:</strong></label>
                    <p style="padding: 10px; background: #f5f5f5; border-radius: 8px; margin-top: 5px;">${apt.patientName}</p>
                </div>
                <div class="form-group">
                    <label><strong>Dentist:</strong></label>
                    <p style="padding: 10px; background: #f5f5f5; border-radius: 8px; margin-top: 5px;">${apt.dentistName}</p>
                </div>
                <div class="form-group">
                    <label><strong>Date:</strong></label>
                    <p style="padding: 10px; background: #f5f5f5; border-radius: 8px; margin-top: 5px;">${apt.date}</p>
                </div>
                <div class="form-group">
                    <label><strong>Time:</strong></label>
                    <p style="padding: 10px; background: #f5f5f5; border-radius: 8px; margin-top: 5px;">${apt.time}</p>
                </div>
                <div class="form-group">
                    <label><strong>Status:</strong></label>
                    <select id="statusSelect_${apt.appointmentId}" style="padding: 10px; border: 1px solid #ddd; border-radius: 8px; margin-top: 5px; width: 100%; font-size: 14px;">
                        <option value="Scheduled" ${apt.status === 'Scheduled' ? 'selected' : ''}>Scheduled</option>
                        <option value="Completed" ${apt.status === 'Completed' ? 'selected' : ''}>Completed</option>
                        <option value="Cancelled" ${apt.status === 'Cancelled' ? 'selected' : ''}>Cancelled</option>
                    </select>
                </div>
                <div class="form-group">
                    <label><strong>Notes:</strong></label>
                    <p style="padding: 10px; background: #f5f5f5; border-radius: 8px; margin-top: 5px; white-space: pre-wrap;">${apt.notes}</p>
                </div>
                <button class="btn-primary" style="width: 100%; margin-top: 10px;" onclick="updateAppointmentStatus(${apt.appointmentId})">Update Status</button>
            </div>
        `;

        document.getElementById('appointmentDetails').innerHTML = detailsHtml;
        document.getElementById('viewAppointmentModal').style.display = 'block';

    } catch (error) {
        alert('Error loading appointment details: ' + error.message);
        console.error('Error:', error);
    }
}

// ------------------- UPDATE STATUS -------------------

async function updateAppointmentStatus(appointmentId) {
    const statusSelect = document.getElementById('statusSelect_' + appointmentId);

    if (!statusSelect) {
        alert('Status field not found');
        return;
    }

    const newStatus = statusSelect.value;

    try {
        const response = await fetch('/Calendar/UpdateAppointmentStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `appointmentId=${appointmentId}&status=${encodeURIComponent(newStatus)}`
        });

        const result = await response.json();

        if (result.success) {
            alert('Status updated successfully!');
            closeViewModal();
            location.reload();
        } else {
            alert('Failed to update status: ' + result.message);
        }
    } catch (error) {
        alert('Error updating status: ' + error.message);
    }
}

// ------------------- CLOSE MODALS ON OUTSIDE CLICK -------------------

window.onclick = function (event) {
    const modal1 = document.getElementById('appointmentModal');
    const modal2 = document.getElementById('dayAppointmentsModal');
    const modal3 = document.getElementById('viewAppointmentModal');

    if (event.target == modal1) modal1.style.display = "none";
    if (event.target == modal2) modal2.style.display = "none";
    if (event.target == modal3) modal3.style.display = "none";
};
