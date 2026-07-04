import * as signalR from '@microsoft/signalr';

let connection = null;

export function getPresenceConnection() {
  if (connection) {
    return connection;
  }

  const accessToken = localStorage.getItem('accessToken');

  connection = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:5169/hubs/presence', {
      accessTokenFactory: () => localStorage.getItem('accessToken'),
    })
    .withAutomaticReconnect()
    .build();

  return connection;
}

export function stopPresenceConnection() {
  if (connection) {
    connection.stop();
    connection = null;
  }
}