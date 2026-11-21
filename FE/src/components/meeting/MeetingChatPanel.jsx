import React, { useState, useRef, useEffect } from 'react';
import { Send, MessageSquare, User } from 'lucide-react';

export default function MeetingChatPanel({ messages = [], onSendMessage, currentUserId }) {
  const [inputText, setInputText] = useState('');
  const messagesEndRef = useRef(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSend = (e) => {
    e.preventDefault();
    if (!inputText.trim()) return;
    onSendMessage(inputText.trim());
    setInputText('');
  };

  return (
    <div className="flex flex-col h-full bg-gray-800 text-white border-l border-gray-700">
      {/* Header */}
      <div className="p-4 border-b border-gray-700 flex items-center justify-between">
        <div className="flex items-center gap-2 font-semibold">
          <MessageSquare size={18} className="text-red-500" />
          <span>Trò chuyện ({messages.length})</span>
        </div>
      </div>

      {/* Messages List */}
      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        {messages.length === 0 ? (
          <div className="text-center text-gray-400 mt-10 text-sm">
            Chưa có tin nhắn nào. Hãy gửi lời chào đến mọi người!
          </div>
        ) : (
          messages.map((msg, index) => {
            const isMe = `${msg.userId}` === `${currentUserId}`;
            const timeStr = msg.sentAt
              ? new Date(msg.sentAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
              : '';

            return (
              <div
                key={msg.chatId || index}
                className={`flex flex-col ${isMe ? 'items-end' : 'items-start'}`}
              >
                <div className="flex items-center gap-1 mb-1 text-xs text-gray-400">
                  {!isMe && <User size={12} />}
                  <span className="font-medium text-gray-300">
                    {isMe ? 'Bạn' : msg.userFullName || `User_${msg.userId}`}
                  </span>
                  <span>• {timeStr}</span>
                </div>
                <div
                  className={`max-w-[85%] rounded-2xl px-4 py-2 text-sm break-words ${
                    isMe
                      ? 'bg-red-600 text-white rounded-br-none'
                      : 'bg-gray-700 text-gray-100 rounded-bl-none'
                  }`}
                >
                  {msg.message}
                </div>
              </div>
            );
          })
        )}
        <div ref={messagesEndRef} />
      </div>

      {/* Input Form */}
      <form onSubmit={handleSend} className="p-3 border-t border-gray-700 bg-gray-850 flex gap-2">
        <input
          type="text"
          value={inputText}
          onChange={(e) => setInputText(e.target.value)}
          placeholder="Nhập tin nhắn..."
          className="flex-1 bg-gray-700 border border-gray-600 rounded-lg px-3 py-2 text-sm text-white placeholder-gray-400 focus:outline-none focus:border-red-500"
        />
        <button
          type="submit"
          disabled={!inputText.trim()}
          className="px-4 py-2 bg-red-600 hover:bg-red-700 disabled:opacity-50 text-white rounded-lg transition-colors flex items-center justify-center"
        >
          <Send size={16} />
        </button>
      </form>
    </div>
  );
}
