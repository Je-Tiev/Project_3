import { File } from 'lucide-react';
import React, { createContext, useContext, useState, useEffect } from 'react';
import { parseDmyToDate } from '../utils/dateUtils';

const AppContext = createContext(null);

const initialMeetings = [
  { 
    id: 1, 
    date: '07/11/2024',
    dayOfWeek: 'Thứ 5',
    session: 'Buổi sáng',
    time: '08:05',
    title: 'Cuộc họp kiểm thử 4',
    location: 'P 3.2',
    organizer: 'PCT Nguyễn Quang Hùng',
    file_rev: 'tailieu_nhan_1.pdf',
    file_pre: 'tailieu_chuanbi_1.pdf',
    status: 'not_started',
    approved: true,
    roles: ['Dự họp'],
    viewStatus: 'Chưa xem'
  },
  { 
    id: 2, 
    date: '07/11/2024',
    dayOfWeek: 'Thứ 5',
    session: 'Buổi sáng',
    time: '08:30',
    title: 'Cuộc họp kiểm thử 3',
    location: 'P 3.2',
    organizer: 'PCT Lại Văn Hoàn',
    file_rev: 'tailieu_nhan_2.pdf',
    file_pre: 'tailieu_chuanbi_2.pdf',
    status: 'not_started',
    approved: true,
    roles: ['Dự họp'],
    viewStatus: 'Chưa xem'
  },
  { 
    id: 3, 
    date: '08/11/2024',
    dayOfWeek: 'Thứ 6',
    session: 'Buổi sáng',
    time: '08:05',
    title: 'Cuộc họp kiểm thử 5',
    location: 'P 3.2',
    organizer: 'PCT Nguyễn Quang Hùng',
    file_rev: 'tailieu_nhan_3.pdf',
    file_pre: 'tailieu_chuanbi_3.pdf',
    status: 'not_started',
    approved: false,
    roles: ['Dự họp'],
    viewStatus: 'Chưa xem'
  },
  { 
    id: 4, 
    date: '08/11/2024',
    dayOfWeek: 'Thứ 6',
    session: 'Buổi chiều',
    time: '14:00',
    title: 'Họp Ban Thường vụ',
    location: 'P 4.1',
    organizer: 'PCT Trần Văn A',
    file_rev: 'tailieu_nhan_4.pdf',
    file_pre: 'Lê Thị C',
    status: 'ongoing',
    approved: true,
    roles: ['Chủ trì'],
    viewStatus: 'Đã xem'
  },
  { 
    id: 5, 
    date: '09/11/2024',
    dayOfWeek: 'Thứ 7',
    session: 'Buổi sáng',
    time: '09:00',
    title: 'Tổng kết nghị quyết 57 BCT',
    location: 'P 3.2',
    organizer: 'PCT Phạm Văn D',
    file_rev: 'tailieu_nhan_5.pdf',
    file_pre: 'tailieu_chuanbi_5.pdf',
    status: 'postponed',
    approved: false,
    roles: ['Tham gia'],
    viewStatus: 'Chưa xem'
  },
  { 
    id: 6, 
    date: '06/11/2024',
    dayOfWeek: 'Thứ 4',
    session: 'Buổi sáng',
    time: '08:00',
    title: 'Triển khai kế hoạch năm 2026',
    location: 'P 2.1',
    organizer: 'PCT Vũ Thị G',
    file_rev: 'tailieu_nhan_6.pdf',
    file_pre: 'tailieu_chuanbi_6.pdf',
    status: 'completed',
    approved: true,
    roles: ['Dự họp'],
    viewStatus: 'Đã xem'
  }
];

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
    try {
      localStorage.setItem('meetings', JSON.stringify(meetings));
    } catch {}
  }, [meetings]);

  const login = (username, password) => {
    if (username && password.length >= 4) {
      const user = {
        username,
        role: username === 'admin' ? 'admin' : 'user',
        fullName: username === 'admin' ? 'Quản Trị Viên' : 'Người Dùng'
      };
      setCurrentUser(user);
      setIsAuthenticated(true);
      return { success: true };
    }
    return { success: false, message: 'Tên đăng nhập hoặc mật khẩu không đúng' };
  };

  const logout = () => {
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

  const createMeeting = (payload) => {
    // payload: { title, date, time, location, session, organizer, approved, status }
    const newMeeting = {
      id: (meetings[0]?.id || 0) + Math.floor(Math.random() * 100000) + 1,
      date: payload.date,
      dayOfWeek: payload.dayOfWeek || 'Thứ 2',
      session: payload.session || 'Buổi sáng',
      time: payload.time || '08:00',
      title: payload.title || 'Cuộc họp mới',
      location: payload.location || 'P 1.1',
      organizer: payload.organizer || 'Ban tổ chức',
      file_rev: payload.file_rev || '',
      file_rev_url: payload.file_rev_url || '',
      file_pre: payload.file_pre || '',
      file_pre_url: payload.file_pre_url || '',
      status: payload.status || 'not_started',
      approved: typeof payload.approved === 'boolean' ? payload.approved : true,
      roles: payload.roles || ['Dự họp'],
      viewStatus: payload.viewStatus || 'Chưa xem'
    };
    setMeetings(prev => [newMeeting, ...prev]);
    return newMeeting;
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