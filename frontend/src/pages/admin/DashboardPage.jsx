import React, { useState } from 'react';
import { Plus } from 'lucide-react';
import { useApp } from '../../contexts/AppContext';
import SummaryCards from '../../components/admin/SummaryCards';
import MeetingsChart from '../../components/admin/MeetingsChart';
import ParticipationChart from '../../components/admin/ParticipationChart';
import RecentMeetingsTable from '../../components/admin/RecentMeetingsTable';
import MeetingDetailsPage from './MeetingDetailsPage';

const DashboardPage = ({ onCreateMeeting, onViewAllMeetings }) => {
  const { meetings } = useApp();
  const [selectedMeetingId, setSelectedMeetingId] = useState(null);

  if (selectedMeetingId) {
    return <MeetingDetailsPage meetingId={selectedMeetingId} onBack={() => setSelectedMeetingId(null)} />;
  }

  return (
    <div className="p-6">
      {/* Header with Create Button */}
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-text">Dashboard</h1>
        <button
          onClick={onCreateMeeting}
          className="bg-primary hover:bg-primary-dark text-white px-6 py-2 rounded-button font-semibold flex items-center gap-2 transition-colors"
        >
          <Plus className="w-5 h-5" />
          Create New Meeting
        </button>
      </div>

      {/* Summary Cards */}
      <SummaryCards meetings={meetings} />

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-6">
        <MeetingsChart meetings={meetings} />
        <ParticipationChart />
      </div>

      {/* Recent Meetings Table */}
      <div className="mt-6">
        <RecentMeetingsTable
          meetings={meetings}
          onViewMeeting={onViewAllMeetings}
          onMeetingClick={(meetingId) => setSelectedMeetingId(meetingId)}
        />
      </div>
    </div>
  );
};

export default DashboardPage;

