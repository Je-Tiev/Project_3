import React, { useState } from 'react';
import { Calendar, Users, FileText, Vote, StickyNote, Download, Eye } from 'lucide-react';
import { useApp } from '../../contexts/AppContext';

const MeetingDetailsPage = ({ meetingId, onBack }) => {
  const { meetings } = useApp();
  const [activeTab, setActiveTab] = useState('agenda');
  
  // Find meeting by ID or use first one as demo
  const meeting = meetings.find(m => m.id === meetingId) || meetings[0] || {};

  const tabs = [
    { key: 'agenda', label: 'Agenda', icon: Calendar },
    { key: 'participants', label: 'Participants', icon: Users },
    { key: 'documents', label: 'Documents', icon: FileText },
    { key: 'voting', label: 'Voting', icon: Vote },
    { key: 'notes', label: 'Notes', icon: StickyNote }
  ];

  const participants = [
    { id: 1, name: 'Nguyễn Văn A', role: 'Host', attendance: 'Present' },
    { id: 2, name: 'Trần Thị B', role: 'Participant', attendance: 'Present' },
    { id: 3, name: 'Lê Văn C', role: 'Participant', attendance: 'Absent' }
  ];

  const votingTopics = [
    { id: 1, topic: 'Approve budget for Q1 2024', agree: 12, disagree: 2, abstain: 1 },
    { id: 2, topic: 'Hire new staff member', agree: 8, disagree: 5, abstain: 2 }
  ];

  const notes = [
    { id: 1, author: 'Nguyễn Văn A', content: 'Important discussion about budget allocation', timestamp: '2024-11-07 10:30' },
    { id: 2, author: 'Trần Thị B', content: 'Need to follow up on action items', timestamp: '2024-11-07 11:00' }
  ];

  return (
    <div className="p-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-4">
          {onBack && (
            <button
              onClick={onBack}
              className="text-text-light hover:text-text transition-colors"
            >
              ← Back
            </button>
          )}
          <h1 className="text-3xl font-bold text-text">{meeting.title || 'Meeting Details'}</h1>
        </div>
        <button className="bg-primary hover:bg-primary-dark text-white px-6 py-2 rounded-button font-semibold transition-colors">
          Join Meeting
        </button>
      </div>

      {/* Tabs */}
      <div className="bg-white border border-secondary-dark rounded-lg mb-6">
        <div className="flex border-b border-secondary-dark overflow-x-auto">
          {tabs.map((tab) => {
            const Icon = tab.icon;
            const isActive = activeTab === tab.key;
            return (
              <button
                key={tab.key}
                onClick={() => setActiveTab(tab.key)}
                className={`flex items-center gap-2 px-6 py-4 font-semibold text-sm transition-colors whitespace-nowrap ${
                  isActive
                    ? 'text-primary border-b-2 border-primary'
                    : 'text-text-light hover:text-text'
                }`}
              >
                <Icon className="w-5 h-5" />
                <span>{tab.label}</span>
              </button>
            );
          })}
        </div>

        {/* Tab Content */}
        <div className="p-6">
          {activeTab === 'agenda' && (
            <div className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-text-light mb-2">Date</label>
                  <p className="text-text font-medium">{meeting.date || 'N/A'}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-text-light mb-2">Time</label>
                  <p className="text-text font-medium">{meeting.time || 'N/A'}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-text-light mb-2">Location/Room</label>
                  <p className="text-text font-medium">{meeting.location || 'N/A'}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-text-light mb-2">Host</label>
                  <p className="text-text font-medium">{meeting.organizer || 'N/A'}</p>
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-text-light mb-2">Description</label>
                <p className="text-text">{meeting.description || 'No description available.'}</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-text-light mb-2">Status</label>
                <span className={`px-3 py-1 rounded-full text-xs font-semibold ${
                  meeting.status === 'completed' ? 'bg-green-100 text-green-700' :
                  meeting.status === 'ongoing' ? 'bg-orange-100 text-orange-700' :
                  'bg-blue-100 text-blue-700'
                }`}>
                  {meeting.status === 'completed' ? 'Completed' :
                   meeting.status === 'ongoing' ? 'Ongoing' : 'Upcoming'}
                </span>
              </div>
            </div>
          )}

          {activeTab === 'participants' && (
            <div className="space-y-4">
              <div className="bg-white border border-secondary-dark rounded-lg overflow-hidden">
                <table className="w-full">
                  <thead className="bg-secondary">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Name</th>
                      <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Role</th>
                      <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Attendance Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-secondary-dark">
                    {participants.map((p, index) => (
                      <tr key={p.id} className={index % 2 === 0 ? 'bg-white' : 'bg-secondary'}>
                        <td className="px-6 py-4 text-sm text-text font-medium">{p.name}</td>
                        <td className="px-6 py-4 text-sm text-text">{p.role}</td>
                        <td className="px-6 py-4 text-sm">
                          <span className={`px-3 py-1 rounded-full text-xs font-semibold ${
                            p.attendance === 'Present' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                          }`}>
                            {p.attendance}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {activeTab === 'documents' && (
            <div className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {meeting.file_rev && (
                  <div className="bg-secondary border border-secondary-dark rounded-lg p-4">
                    <div className="flex items-center justify-between mb-2">
                      <h3 className="font-semibold text-text">Tài liệu được phát</h3>
                      {meeting.file_rev_url && (
                        <a
                          href={meeting.file_rev_url}
                          target="_blank"
                          rel="noreferrer"
                          className="text-primary hover:text-primary-dark flex items-center gap-1 text-sm"
                        >
                          <Eye className="w-4 h-4" />
                          View
                        </a>
                      )}
                    </div>
                    <p className="text-sm text-text-light">{meeting.file_rev}</p>
                    {meeting.file_rev_url && (
                      <button className="mt-2 text-primary hover:text-primary-dark flex items-center gap-1 text-sm">
                        <Download className="w-4 h-4" />
                        Download
                      </button>
                    )}
                  </div>
                )}
                {meeting.file_pre && (
                  <div className="bg-secondary border border-secondary-dark rounded-lg p-4">
                    <div className="flex items-center justify-between mb-2">
                      <h3 className="font-semibold text-text">Tài liệu chuẩn bị</h3>
                      {meeting.file_pre_url && (
                        <a
                          href={meeting.file_pre_url}
                          target="_blank"
                          rel="noreferrer"
                          className="text-primary hover:text-primary-dark flex items-center gap-1 text-sm"
                        >
                          <Eye className="w-4 h-4" />
                          View
                        </a>
                      )}
                    </div>
                    <p className="text-sm text-text-light">{meeting.file_pre}</p>
                    {meeting.file_pre_url && (
                      <button className="mt-2 text-primary hover:text-primary-dark flex items-center gap-1 text-sm">
                        <Download className="w-4 h-4" />
                        Download
                      </button>
                    )}
                  </div>
                )}
              </div>
              {!meeting.file_rev && !meeting.file_pre && (
                <p className="text-text-light text-center py-8">No documents attached to this meeting.</p>
              )}
            </div>
          )}

          {activeTab === 'voting' && (
            <div className="space-y-4">
              {votingTopics.map((topic) => (
                <div key={topic.id} className="bg-secondary border border-secondary-dark rounded-lg p-4">
                  <h3 className="font-semibold text-text mb-3">{topic.topic}</h3>
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-text-light">Agree</span>
                      <div className="flex items-center gap-2">
                        <div className="w-48 bg-secondary-dark rounded-full h-2">
                          <div
                            className="bg-green-600 h-2 rounded-full"
                            style={{ width: `${(topic.agree / (topic.agree + topic.disagree + topic.abstain)) * 100}%` }}
                          />
                        </div>
                        <span className="text-sm font-semibold text-text w-8">{topic.agree}</span>
                      </div>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-text-light">Disagree</span>
                      <div className="flex items-center gap-2">
                        <div className="w-48 bg-secondary-dark rounded-full h-2">
                          <div
                            className="bg-red-600 h-2 rounded-full"
                            style={{ width: `${(topic.disagree / (topic.agree + topic.disagree + topic.abstain)) * 100}%` }}
                          />
                        </div>
                        <span className="text-sm font-semibold text-text w-8">{topic.disagree}</span>
                      </div>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-text-light">Abstain</span>
                      <div className="flex items-center gap-2">
                        <div className="w-48 bg-secondary-dark rounded-full h-2">
                          <div
                            className="bg-gray-400 h-2 rounded-full"
                            style={{ width: `${(topic.abstain / (topic.agree + topic.disagree + topic.abstain)) * 100}%` }}
                          />
                        </div>
                        <span className="text-sm font-semibold text-text w-8">{topic.abstain}</span>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
              {votingTopics.length === 0 && (
                <p className="text-text-light text-center py-8">No voting topics for this meeting.</p>
              )}
            </div>
          )}

          {activeTab === 'notes' && (
            <div className="space-y-4">
              {notes.map((note) => (
                <div key={note.id} className="bg-secondary border border-secondary-dark rounded-lg p-4">
                  <div className="flex items-center justify-between mb-2">
                    <span className="font-semibold text-text">{note.author}</span>
                    <span className="text-sm text-text-light">{note.timestamp}</span>
                  </div>
                  <p className="text-text">{note.content}</p>
                </div>
              ))}
              <button className="w-full border-2 border-dashed border-secondary-dark rounded-lg p-4 text-text-light hover:text-text hover:border-primary transition-colors">
                + Add New Note
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default MeetingDetailsPage;

