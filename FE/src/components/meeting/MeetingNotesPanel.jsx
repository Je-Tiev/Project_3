import React, { useState, useEffect, useRef } from 'react';
import { Edit3, Check, Cloud, AlertCircle } from 'lucide-react';
import { apiCall } from '../../utils/api';

export default function MeetingNotesPanel({ meetingId }) {
  const [content, setContent] = useState('');
  const [saveStatus, setSaveStatus] = useState('idle'); // 'idle' | 'saving' | 'saved' | 'error'
  const [lastSavedTime, setLastSavedTime] = useState(null);
  const isFirstLoad = useRef(true);
  const debounceTimer = useRef(null);

  // Fetch initial note
  useEffect(() => {
    let isMounted = true;
    const fetchNote = async () => {
      try {
        const res = await apiCall(`/Notes/meeting/${meetingId}`, { method: 'GET' });
        if (isMounted && res && res.content !== undefined) {
          setContent(res.content || '');
          if (res.updatedAt) {
            setLastSavedTime(new Date(res.updatedAt).toLocaleTimeString('vi-VN'));
          }
        }
      } catch (err) {
        console.warn('Chưa có ghi chú trước đó:', err);
      } finally {
        isFirstLoad.current = false;
      }
    };

    if (meetingId) {
      fetchNote();
    }

    return () => {
      isMounted = false;
      if (debounceTimer.current) clearTimeout(debounceTimer.current);
    };
  }, [meetingId]);

  // Handle content change with debounce auto-save
  const handleContentChange = (e) => {
    const val = e.target.value;
    setContent(val);

    if (isFirstLoad.current) return;

    setSaveStatus('saving');
    if (debounceTimer.current) clearTimeout(debounceTimer.current);

    debounceTimer.current = setTimeout(async () => {
      try {
        await apiCall(`/Notes/meeting/${meetingId}`, {
          method: 'POST',
          body: JSON.stringify({ content: val }),
        });
        setSaveStatus('saved');
        setLastSavedTime(new Date().toLocaleTimeString('vi-VN'));
      } catch (err) {
        console.error('Lỗi tự động lưu ghi chú:', err);
        setSaveStatus('error');
      }
    }, 1000);
  };

  return (
    <div className="flex flex-col h-full bg-gray-800 text-white border-l border-gray-700">
      {/* Header */}
      <div className="p-4 border-b border-gray-700 flex items-center justify-between">
        <div className="flex items-center gap-2 font-semibold">
          <Edit3 size={18} className="text-red-500" />
          <span>Sổ tay ghi chú cá nhân</span>
        </div>
        <div className="text-xs flex items-center gap-1.5 text-gray-400">
          {saveStatus === 'saving' && (
            <span className="flex items-center gap-1 text-yellow-400">
              <Cloud size={12} className="animate-pulse" /> Đang lưu...
            </span>
          )}
          {saveStatus === 'saved' && (
            <span className="flex items-center gap-1 text-green-400">
              <Check size={12} /> Đã lưu {lastSavedTime}
            </span>
          )}
          {saveStatus === 'error' && (
            <span className="flex items-center gap-1 text-red-400">
              <AlertCircle size={12} /> Lỗi lưu
            </span>
          )}
        </div>
      </div>

      {/* Editor Body */}
      <div className="flex-1 p-4 flex flex-col">
        <p className="text-xs text-gray-400 mb-2">
          Ghi chú này là riêng tư của bạn, được lưu tự động trên hệ thống và gắn liền với phiên họp này.
        </p>
        <textarea
          value={content}
          onChange={handleContentChange}
          placeholder="Nhập các nội dung quan trọng, chỉ đạo của lãnh đạo hoặc ý kiến cá nhân..."
          className="flex-1 w-full bg-gray-750 border border-gray-700 rounded-xl p-4 text-sm text-gray-100 placeholder-gray-500 focus:outline-none focus:border-red-500 resize-none font-sans leading-relaxed"
        />
      </div>
    </div>
  );
}
