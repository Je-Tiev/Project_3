import React, { useState } from 'react';
import { Eye, Edit, Trash2, FileText, Plus } from 'lucide-react';
import { useApp } from '../../contexts/AppContext';
import MeetingDetailsPage from './MeetingDetailsPage';

const MeetingsListPage = () => {
  const { meetings, getFilteredMeetings } = useApp();
  const [statusFilter, setStatusFilter] = useState('all');
  const [dateRange, setDateRange] = useState({ start: '', end: '' });
  const [roomFilter, setRoomFilter] = useState('');
  const [selectedMeetingId, setSelectedMeetingId] = useState(null);

  const allMeetings = getFilteredMeetings();

  if (selectedMeetingId) {
    return <MeetingDetailsPage meetingId={selectedMeetingId} onBack={() => setSelectedMeetingId(null)} />;
  }

  const getStatusBadge = (status) => {
    const config = {
      not_started: { bg: 'bg-blue-100', text: 'text-blue-700', label: 'Upcoming' },
      ongoing: { bg: 'bg-orange-100', text: 'text-orange-700', label: 'Ongoing' },
      completed: { bg: 'bg-green-100', text: 'text-green-700', label: 'Completed' },
      postponed: { bg: 'bg-gray-100', text: 'text-gray-700', label: 'Postponed' }
    };
    const c = config[status] || config.not_started;
    return (
      <span className={`px-3 py-1 rounded-full text-xs font-semibold ${c.bg} ${c.text}`}>
        {c.label}
      </span>
    );
  };

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-text">Meetings List</h1>
        <button className="bg-primary hover:bg-primary-dark text-white px-6 py-2 rounded-button font-semibold flex items-center gap-2 transition-colors">
          <Plus className="w-5 h-5" />
          Create New Meeting
        </button>
      </div>

      {/* Filters */}
      <div className="bg-white border border-secondary-dark rounded-lg p-4 mb-6">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label className="block text-sm font-medium text-text mb-2">Status</label>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="w-full px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
            >
              <option value="all">All Status</option>
              <option value="not_started">Upcoming</option>
              <option value="ongoing">Ongoing</option>
              <option value="completed">Completed</option>
              <option value="postponed">Postponed</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-text mb-2">Start Date</label>
            <input
              type="text"
              value={dateRange.start}
              onChange={(e) => setDateRange({ ...dateRange, start: e.target.value })}
              placeholder="dd/mm/yyyy"
              className="w-full px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-text mb-2">End Date</label>
            <input
              type="text"
              value={dateRange.end}
              onChange={(e) => setDateRange({ ...dateRange, end: e.target.value })}
              placeholder="dd/mm/yyyy"
              className="w-full px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-text mb-2">Room</label>
            <input
              type="text"
              value={roomFilter}
              onChange={(e) => setRoomFilter(e.target.value)}
              placeholder="Filter by room"
              className="w-full px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
            />
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white border border-secondary-dark rounded-lg shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-secondary">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Date</th>
                <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Time</th>
                <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Room</th>
                <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Host</th>
                <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Documents</th>
                <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Status</th>
                <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-secondary-dark">
              {allMeetings.map((meeting, index) => (
                <tr
                  key={meeting.id}
                  onClick={() => setSelectedMeetingId(meeting.id)}
                  className={`${index % 2 === 0 ? 'bg-white' : 'bg-secondary'} cursor-pointer hover:bg-primary/10 transition-colors`}
                >
                  <td className="px-6 py-4 text-sm text-text">{meeting.date}</td>
                  <td className="px-6 py-4 text-sm text-text">{meeting.time}</td>
                  <td className="px-6 py-4 text-sm text-text">{meeting.location || 'N/A'}</td>
                  <td className="px-6 py-4 text-sm text-text">{meeting.organizer || 'N/A'}</td>
                  <td className="px-6 py-4 text-sm">
                    {meeting.file_rev || meeting.file_pre ? (
                      <span className="text-primary flex items-center gap-1">
                        <FileText className="w-4 h-4" />
                        {[meeting.file_rev, meeting.file_pre].filter(Boolean).length} files
                      </span>
                    ) : (
                      <span className="text-text-light">No documents</span>
                    )}
                  </td>
                  <td className="px-6 py-4">{getStatusBadge(meeting.status)}</td>
                  <td className="px-6 py-4" onClick={(e) => e.stopPropagation()}>
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => setSelectedMeetingId(meeting.id)}
                        className="p-2 text-primary hover:bg-secondary rounded-button transition-colors"
                        title="View"
                      >
                        <Eye className="w-4 h-4" />
                      </button>
                      <button className="p-2 text-primary hover:bg-secondary rounded-button transition-colors" title="Add Notes">
                        <FileText className="w-4 h-4" />
                      </button>
                      <button className="p-2 text-primary hover:bg-secondary rounded-button transition-colors" title="Edit">
                        <Edit className="w-4 h-4" />
                      </button>
                      <button className="p-2 text-red-600 hover:bg-secondary rounded-button transition-colors" title="Delete">
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default MeetingsListPage;

