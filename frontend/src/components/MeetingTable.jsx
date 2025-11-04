import React from 'react';
import { useApp } from '../contexts/AppContext';

const MeetingTable = () => {
  const { getFilteredMeetings } = useApp();
  const filteredMeetings = getFilteredMeetings();

  const getStatusBadge = (meeting) => {
    if (meeting.approved) {
      return (
        <span className="inline-block bg-green-500 text-white text-xs px-3 py-1 rounded-full font-semibold">
          ĐÃ HỌP
        </span>
      );
    }
    return (
      <span className="inline-block bg-red-500 text-white text-xs px-3 py-1 rounded-full font-semibold">
        Chưa bắt đầu
      </span>
    );
  };

  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead className="bg-gray-100 border-b-2 border-gray-200">
            <tr>
              <th className="px-4 py-3 text-left text-sm font-bold text-gray-700">Ngày</th>
              <th className="px-4 py-3 text-left text-sm font-bold text-gray-700">Thời gian</th>
              <th className="px-4 py-3 text-left text-sm font-bold text-gray-700">Giờ</th>
              <th className="px-4 py-3 text-left text-sm font-bold text-gray-700">Nội dung</th>
              <th className="px-4 py-3 text-left text-sm font-bold text-gray-700">Địa điểm</th>
              <th className="px-4 py-3 text-left text-sm font-bold text-gray-700">LĐ chủ trì</th>
              <th className="px-4 py-3 text-left text-sm font-bold text-gray-700">Tài liệu được phát</th>
              <th className="px-4 py-3 text-left text-sm font-bold text-gray-700">Tài liệu chuẩn bị</th>
              <th className="px-4 py-3 text-left text-sm font-bold text-gray-700">Vai trò</th>
              <th className="px-4 py-3 text-left text-sm font-bold text-gray-700">Trạng thái</th>
              <th className="px-4 py-3 text-left text-sm font-bold text-gray-700">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {filteredMeetings.map((meeting, index) => (
              <tr 
                key={meeting.id} 
                className={`border-b ${index % 2 === 0 ? 'bg-white' : 'bg-gray-50'} hover:bg-blue-50 transition-colors`}
              >
                <td className="px-4 py-4 text-sm">
                  <div>{meeting.dayOfWeek},</div>
                  <div>{meeting.date}</div>
                </td>
                <td className="px-4 py-4 text-sm">{meeting.session}</td>
                <td className="px-4 py-4 text-sm font-semibold">{meeting.time}</td>
                <td className="px-4 py-4">
                  <a href="#" className="text-blue-600 hover:text-blue-800 text-sm hover:underline">
                    {meeting.title}
                  </a>
                </td>
                <td className="px-4 py-4 text-sm">{meeting.location}</td>
                <td className="px-4 py-4 text-sm">{meeting.organizer}</td>
                <td className="px-4 py-4 text-sm">
                  {meeting.file_rev_url ? (
                    <a href={meeting.file_rev_url} target="_blank" rel="noreferrer" className="text-blue-600 hover:underline">
                      {meeting.file_rev || 'Tài liệu'}
                    </a>
                  ) : (
                    meeting.file_rev || ''
                  )}
                </td>
                <td className="px-4 py-4 text-sm">
                  {meeting.file_pre_url ? (
                    <a href={meeting.file_pre_url} target="_blank" rel="noreferrer" className="text-blue-600 hover:underline">
                      {meeting.file_pre || 'Tài liệu'}
                    </a>
                  ) : (
                    meeting.file_pre || ''
                  )}
                </td>
                <td className="px-4 py-4">
                  <span className="inline-block bg-green-100 text-green-700 text-xs px-3 py-1 rounded-full font-semibold mr-1">
                    {meeting.roles[0]}
                  </span>
                  <span className="inline-block bg-blue-100 text-blue-700 text-xs px-3 py-1 rounded-full font-semibold">
                    {meeting.viewStatus}
                  </span>
                </td>
                <td className="px-4 py-4">
                  {getStatusBadge(meeting)}
                </td>
                <td className="px-4 py-4">
                  <button className="bg-green-600 hover:bg-green-700 text-white text-xs px-4 py-2 rounded-lg font-semibold transition-colors">
                    Thêm ghi chú
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default MeetingTable;