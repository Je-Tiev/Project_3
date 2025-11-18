import { File } from 'lucide-react';
import React, { createContext, useContext, useState, useEffect } from 'react';
import { parseDmyToDate } from '../utils/dateUtils';
import { apiCall } from '../utils/api';

const AppContext = createContext(null);

// const initialMeetings = [
//   { 
//     id: 1, 
//     date: '07/11/2024',
//     dayOfWeek: 'Thứ 5',
//     session: 'Buổi sáng',
//     time: '08:05',
//     title: 'Cuộc họp kiểm thử 4',
//     location: 'P 3.2',
//     organizer: 'PCT Nguyễn Quang Hùng',
//     file_rev: 'tailieu_nhan_1.pdf',
//     file_pre: 'tailieu_chuanbi_1.pdf',
//     status: 'not_started',
//     approved: true,
//     roles: ['Dự họp'],
//     viewStatus: 'Chưa xem'
//   },
//   { 
//     id: 2, 
//     date: '07/11/2024',
//     dayOfWeek: 'Thứ 5',
//     session: 'Buổi sáng',
//     time: '08:30',
//     title: 'Cuộc họp kiểm thử 3',
//     location: 'P 3.2',
//     organizer: 'PCT Lại Văn Hoàn',
//     file_rev: 'tailieu_nhan_2.pdf',
//     file_pre: 'tailieu_chuanbi_2.pdf',
//     status: 'not_started',
//     approved: true,
//     roles: ['Dự họp'],
//     viewStatus: 'Chưa xem'
//   },
//   { 
//     id: 3, 
//     date: '08/11/2024',
//     dayOfWeek: 'Thứ 6',
//     session: 'Buổi sáng',
//     time: '08:05',
//     title: 'Cuộc họp kiểm thử 5',
//     location: 'P 3.2',
//     organizer: 'PCT Nguyễn Quang Hùng',
//     file_rev: 'tailieu_nhan_3.pdf',
//     file_pre: 'tailieu_chuanbi_3.pdf',
//     status: 'not_started',
//     approved: false,
//     roles: ['Dự họp'],
//     viewStatus: 'Chưa xem'
//   },
//   { 
//     id: 4, 
//     date: '08/11/2024',
//     dayOfWeek: 'Thứ 6',
//     session: 'Buổi chiều',
//     time: '14:00',
//     title: 'Họp Ban Thường vụ',
//     location: 'P 4.1',
//     organizer: 'PCT Trần Văn A',
//     file_rev: 'tailieu_nhan_4.pdf',
//     file_pre: 'Lê Thị C',
//     status: 'ongoing',
//     approved: true,
//     roles: ['Chủ trì'],
//     viewStatus: 'Đã xem'
//   },
//   { 
//     id: 5, 
//     date: '09/11/2024',
//     dayOfWeek: 'Thứ 7',
//     session: 'Buổi sáng',
//     time: '09:00',
//     title: 'Tổng kết nghị quyết 57 BCT',
//     location: 'P 3.2',
//     organizer: 'PCT Phạm Văn D',
//     file_rev: 'tailieu_nhan_5.pdf',
//     file_pre: 'tailieu_chuanbi_5.pdf',
//     status: 'postponed',
//     approved: false,
//     roles: ['Tham gia'],
//     viewStatus: 'Chưa xem'
//   },
//   { 
//     id: 6, 
//     date: '06/11/2024',
//     dayOfWeek: 'Thứ 4',
//     session: 'Buổi sáng',
//     time: '08:00',
//     title: 'Triển khai kế hoạch năm 2026',
//     location: 'P 2.1',
//     organizer: 'PCT Vũ Thị G',
//     file_rev: 'tailieu_nhan_6.pdf',
//     file_pre: 'tailieu_chuanbi_6.pdf',
//     status: 'completed',
//     approved: true,
//     roles: ['Dự họp'],
//     viewStatus: 'Đã xem'
//   }
// ];

export const AppProvider = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);
  const [deviceStatus, setDeviceStatus] = useState('ready');
  const [currentMeeting, setCurrentMeeting] = useState(null);
  const [meetings, setMeetings] = useState(() => {
    try {
      const saved = localStorage.getItem('meetings');
      return saved ? JSON.parse(saved) : initialMeetings;
    } catch {
      return initialMeetings;
    }
  });
  const [currentTime, setCurrentTime] = useState(new Date());
  const [activeTab, setActiveTab] = useState('not_started');
  const [searchFilters, setSearchFilters] = useState({
    startDate: '04/11/2024',
    endDate: '10/11/2024',
    filterOption: 'all'
  });

  useEffect(() => {
    const timer = setInterval(() => setCurrentTime(new Date()), 1000);
    return () => clearInterval(timer);
  }, []);

  useEffect(() => {
    // Fetch meetings khi app mount
    if (isAuthenticated) {
      fetchMeetings();
    }
  }, [isAuthenticated]);

  useEffect(() => {
    try {
      localStorage.setItem('meetings', JSON.stringify(meetings));
    } catch {}
  }, [meetings]);

  useEffect(() => {
    // Chạy 1 lần khi app tải
    const token = localStorage.getItem('token');
    const savedUser = localStorage.getItem('currentUser');

    if (token && savedUser) {
      try {
        // Khôi phục lại state
        setCurrentUser(JSON.parse(savedUser));
        setIsAuthenticated(true);
      } catch (e) {
        logout(); // Dọn dẹp nếu data hỏng
      }
    }
  }, []);

// Hàm đăng nhập
const login = async (username, password) => {
  try {
    const response = await apiCall('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ username, password })
    });
    
    localStorage.setItem('token', response.token);
    localStorage.setItem('refreshToken', response.refreshToken);
    
    const user = {
      username: response.username,
      role: response.role,
      fullName: response.fullName,
      userId: response.userId
    };
    //Lưu user vào localStorage
    localStorage.setItem('currentUser', JSON.stringify(user));
    // cập nhật state với user hiện tại
    setCurrentUser(user);
    setIsAuthenticated(true);
    return { success: true };
  } catch (error) {
    return { success: false, message: error.message };
  }
};

// Hàm lấy danh sách cuộc họp từ API
const fetchMeetings = async () => {
  try {
    const response = await apiCall('/meetings', { method: 'GET' });
    // API returns { value: [...], Count: ... }
    const meetingsList = response.value || response || [];
    console.log('📥 Fetched meetings from API:', meetingsList.length, 'items');
    // Convert API format to frontend format
    const convertedMeetings = meetingsList.map(m => {
      const startDate = new Date(m.startTime);
      const dayOfWeek = ['Chủ nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'][startDate.getDay()];
      return {
        id: m.meetingId,
        title: m.title,
        description: m.description || '',
        startTime: m.startTime,
        endTime: m.endTime,
        location: m.location || '',
        status: ['not_started', 'ongoing', 'completed', 'postponed'][m.status] || 'not_started',
        date: startDate.toLocaleDateString('vi-VN'),
        dayOfWeek: dayOfWeek,
        time: startDate.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }),
        organizer: '',
        file_rev: '',
        file_pre: '',
        approved: m.status === 0, // not_started = approved
        roles: ['Dự họp'],
        viewStatus: 'Chưa xem'
      };
    });
    setMeetings(convertedMeetings);
  } catch (error) {
    console.error('❌ Lỗi tải cuộc họp:', error);
  }
};

  const logout = () => {
    // Xoá token và user khỏi localStorage
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('currentUser');
    setCurrentUser(null);
    setIsAuthenticated(false);
    setCurrentMeeting(null);
    setDeviceStatus('ready');
  };

  const startMeeting = (meeting) => {
    setCurrentMeeting(meeting);
    setDeviceStatus('in_meeting');
  };

  const endMeeting = () => {
    setCurrentMeeting(null);
    setDeviceStatus('ready');
  };

  const createMeeting = async (payload) => {
    try {
      // Convert local format to API format (MeetingCreateDto)
      const [day, month, year] = payload.date.split('/').map(Number);
      const [hour, minute] = (payload.time || '08:00').split(':').map(Number);
      
      const startTime = new Date(year, month - 1, day, hour, minute);
      const endTime = new Date(year, month - 1, day, hour + 1, minute); // Mặc định +1 giờ
      
      const meetingData = {
        title: payload.title,
        description: payload.location || '',
        startTime: startTime.toISOString(),
        endTime: endTime.toISOString(),
        location: payload.location || '',
        participants: payload.participants || [] // Cần được populate từ form
      };
      
      const response = await apiCall('/meetings', {
        method: 'POST',
        body: JSON.stringify(meetingData)
      });
      
      // Fetch lại meetings từ server
      await fetchMeetings();
      return { success: true, data: response };
    } catch (error) {
      console.error('Lỗi tạo cuộc họp:', error);
      return { success: false, error: error.message };
    }
  };

  const updateSearchFilters = (filters) => {
    setSearchFilters(prev => ({ ...prev, ...filters }));
  };

  const getFilteredMeetings = () => {
    const start = parseDmyToDate(searchFilters.startDate);
    const end = parseDmyToDate(searchFilters.endDate);
    const endOfDay = end ? new Date(end.getFullYear(), end.getMonth(), end.getDate(), 23, 59, 59, 999) : null;

    return meetings.filter(meeting => {
      // status tab
      if (activeTab !== 'all' && meeting.status !== activeTab) {
        return false;
      }
      // approval filter
      if (searchFilters.filterOption === 'approved' && !meeting.approved) {
        return false;
      }
      if (searchFilters.filterOption === 'pending' && meeting.approved) {
        return false;
      }
      // date range filter (meeting.date is dd/mm/yyyy)
      const mDate = parseDmyToDate(meeting.date);
      if (!mDate) return false;
      if (start && mDate < start) return false;
      if (endOfDay && mDate > endOfDay) return false;
      return true;
    });
  };

  const value = {
    isAuthenticated,
    currentUser,
    deviceStatus,
    currentMeeting,
    meetings,
    currentTime,
    activeTab,
    setActiveTab,
    searchFilters,
    updateSearchFilters,
    getFilteredMeetings,
    createMeeting,
    login,
    logout,
    startMeeting,
    endMeeting
  };

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
};

export const useApp = () => {
  const context = useContext(AppContext);
  if (!context) {
    throw new Error('useApp must be used within AppProvider');
  }
  return context;
};