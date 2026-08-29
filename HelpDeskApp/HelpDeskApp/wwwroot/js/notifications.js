"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

connection.on(
    "ReceiveTicketNotification",
    function (ticketId, message) {
        showNotification(ticketId, message);
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