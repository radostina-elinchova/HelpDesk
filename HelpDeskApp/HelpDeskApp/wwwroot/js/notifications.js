"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

connection.on(
    "ReceiveTicketNotification",
    function (ticketId, message) {
        showNotification(ticketId, message);
        updateNotificationCount();

    });

connection.start()
    .catch(function (error) {
        console.error(error.toString());
    });

function showNotification(ticketId, message) {
    const container =
        document.getElementById("notification-container");

    if (!container) {
        return;
    }

    const notification =
        document.createElement("div");

    notification.className =
        "alert alert-info alert-dismissible fade show";

    const messageElement =
        document.createElement("span");

    messageElement.textContent = message;

    const link =
        document.createElement("a");

    link.href = `/Ticket/Details/${ticketId}`;
    link.textContent = " Open ticket";
    link.className = "alert-link";

    const closeButton =
        document.createElement("button");

    closeButton.type = "button";
    closeButton.className = "btn-close";
    closeButton.setAttribute(
        "data-bs-dismiss",
        "alert");

    notification.appendChild(messageElement);
    notification.appendChild(link);
    notification.appendChild(closeButton);

    container.prepend(notification);
}
function updateNotificationCount() {
    const countElement =
        document.getElementById("notification-count");

    if (!countElement) {
        return;
    }

    const currentCount =
        Number.parseInt(
            countElement.textContent,
            10) || 0;

    countElement.textContent =
        currentCount + 1;

    countElement.classList.remove("d-none");
}