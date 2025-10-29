import React, { useEffect } from 'react';
import { useApp } from '../contexts/AppContext';
import Header from '../components/Header';

const MeetingDetailPage = () => {
  const { isAuthenticated, currentMeeting } = useApp();

  useEffect(() => {
    if (!isAuthenticated) {
      window.location.hash = '/login';
    }
  }, [isAuthenticated]);

  if (!isAuthenticated || !currentMeeting) {
    return null;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <Header title={`Chi tiết: ${currentMeeting.title}`} showBackButton={true} />
      
      <div className="max-w-7xl mx-auto p-6">
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-2xl font-bold mb-4">Thông tin cuộc họp</h2>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-gray-600">Ngày:</p>
              <p className="font-semibold">{currentMeeting.date}</p>
            </div>
            <div>
              <p className="text-gray-600">Giờ:</p>
              <p className="font-semibold">{currentMeeting.time}</p>
            </div>
            <div>
              <p className="text-gray-600">Địa điểm:</p>
              <p className="font-semibold">{currentMeeting.location}</p>
            </div>
            <div>
              <p className="text-gray-600">Trạng thái:</p>
              <p className="font-semibold">{currentMeeting.status}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MeetingDetailPage;