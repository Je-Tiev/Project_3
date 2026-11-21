import React, { useEffect } from 'react';
import { useApp } from '../contexts/AppContext';
import Header from '../components/Header';
import SearchBar from '../components/SearchBar';
import MeetingTabs from '../components/MeetingTabs';
import MeetingTable from '../components/MeetingTable';
import { useNavigate } from 'react-router-dom';

const HomePage = () => {
  const { isAuthenticated, currentUser } = useApp();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
    }
  }, [isAuthenticated, navigate]);

  if (!isAuthenticated) {
    return null;
  }

  const role = currentUser?.role?.toLowerCase();
  const isAdmin = role === 'admin' || role === 'superadmin';

  return (
    <div className="min-h-screen bg-gray-50">
      <Header title="Phòng họp không giấy Đại học Bách khoa Hà Nội" />
      
      <div className="max-w-7xl mx-auto p-6">
        {isAdmin && (
          <div className="mb-6 flex justify-between items-center bg-white p-4 rounded-xl shadow-sm border border-gray-200">
            <div>
              <h2 className="text-base font-bold text-gray-800">Quản Trị Hệ Thống</h2>
              <p className="text-xs text-gray-500">Tạo lịch họp mới, điều phối phòng họp vật lý và quản trị đại biểu</p>
            </div>
            <div className="flex gap-3">
              <button
                onClick={() => navigate('/admin/create-meeting')}
                className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-lg text-sm font-semibold flex items-center gap-1.5 shadow-sm transition-colors"
              >
                + Tạo Cuộc Họp Mới
              </button>
              <button
                onClick={() => navigate('/admin')}
                className="bg-gray-100 hover:bg-gray-200 text-gray-700 px-4 py-2 rounded-lg text-sm font-semibold transition-colors"
              >
                Trang Quản Trị
              </button>
            </div>
          </div>
        )}
        <SearchBar />
        <MeetingTabs />
        <MeetingTable />

        <div className="mt-6 text-center text-sm text-gray-600">
          Bản quyền thuộc về: Trường Công nghệ Thông tin và Truyền thông - Đại học Bách Khoa Hà Nội
        </div>
      </div>
    </div>
  );
};

export default HomePage;
