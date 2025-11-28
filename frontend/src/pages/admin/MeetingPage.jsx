import React, { useEffect, useState, useRef } from "react";
import * as signalR from "@microsoft/signalr";

const MeetingPage = () => {
  const [connection, setConnection] = useState(null);
  const [messages, setMessages] = useState([]);
  const [users, setUsers] = useState([]);
  const messageRef = useRef();

  useEffect(() => {
    const token = localStorage.getItem("token");

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("/meetinghub", {
        accessTokenFactory: () => token, // ✅ Auth tự động
      })
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);

    return () => {
      newConnection.stop();
    };
  }, []);

  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          // ✅ JOIN ROOM từ URL params hoặc state
          const urlParams = new URLSearchParams(window.location.search);
          const meetingId = urlParams.get("meetingId") || 1; // Test ID

          connection
            .invoke("JoinRoom", parseInt(meetingId))
            .then(() => console.log("✅ Joined meeting!"))
            .catch((err) => console.error("❌ Join failed:", err));
        })
        .catch((err) => console.error("❌ Connection failed:", err));

      // ✅ NHẬN EVENTS
      connection.on("UserJoined", (user) => {
        console.log("👤", user.fullName, "joined");
        setUsers((prev) => [...prev, user]);
      });

      connection.on("UserLeft", (user) => {
        console.log("👋", user.userId, "left");
        setUsers((prev) => prev.filter((u) => u.userId !== user.userId));
      });

      connection.on("ReceiveMessage", (msg) => {
        setMessages((prev) => [...prev, msg]);
      });
    }
  }, [connection]);

  const sendMessage = async () => {
    if (connection && messageRef.current.value) {
      const meetingId = 1; // Từ URL params
      await connection.invoke(
        "SendMessage",
        meetingId,
        messageRef.current.value
      );
      messageRef.current.value = "";
    }
  };

  return (
    <div className="p-6 max-w-7xl mx-auto">
      <h1 className="text-3xl font-bold mb-6">Meeting Room</h1>

      {/* Users List */}
      <div className="grid grid-cols-4 gap-6 mb-6">
        <div className="col-span-1 bg-white rounded-lg p-4 border">
          <h3 className="font-semibold mb-3">Participants ({users.length})</h3>
          <ul className="space-y-2">
            {users.map((user) => (
              <li
                key={user.connectionId}
                className="flex items-center gap-2 text-sm"
              >
                <div className="w-8 h-8 bg-primary rounded-full flex items-center justify-center text-white">
                  {user.fullName[0]}
                </div>
                <span>{user.fullName}</span>
              </li>
            ))}
          </ul>
        </div>

        {/* Video Area */}
        <div className="col-span-3 bg-gray-100 rounded-lg flex items-center justify-center">
          <div className="text-center text-gray-500">
            <div className="w-32 h-32 bg-blue-200 rounded-full mx-auto mb-4 flex items-center justify-center">
              📹
            </div>
            <p>Video call area</p>
          </div>
        </div>
      </div>

      {/* Chat */}
      <div className="bg-white rounded-lg border p-6">
        <h3 className="font-semibold mb-4">Chat</h3>
        <div className="h-64 overflow-y-auto space-y-3 mb-4 p-4 bg-gray-50 rounded-lg">
          {messages.map((msg, idx) => (
            <div key={idx} className="flex gap-2">
              <div className="font-semibold text-primary">{msg.userName}:</div>
              <div>{msg.message}</div>
              <div className="text-xs text-gray-500 ml-auto">
                {new Date(msg.timestamp).toLocaleTimeString()}
              </div>
            </div>
          ))}
        </div>

        <div className="flex gap-2">
          <input
            ref={messageRef}
            type="text"
            placeholder="Nhập tin nhắn..."
            className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary"
            onKeyPress={(e) => e.key === "Enter" && sendMessage()}
          />
          <button
            onClick={sendMessage}
            className="px-6 py-2 bg-primary text-white rounded-lg hover:bg-primary-dark"
          >
            Gửi
          </button>
        </div>
      </div>
    </div>
  );
};

export default MeetingPage;
