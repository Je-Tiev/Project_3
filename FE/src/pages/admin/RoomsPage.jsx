import React, { useState, useEffect, useCallback } from 'react';
import { Plus, Trash2, Eye, X, Loader2, AlertCircle, CheckCircle2 } from 'lucide-react';
import { apiCall } from '../../utils/api';

const RoomsPage = () => {
  const [rooms, setRooms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  // Modal tạo phòng
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [creating, setCreating] = useState(false);
  const [newRoom, setNewRoom] = useState({
    roomName: '',
    location: '',
    capacity: 20,
    equipment: '',
    status: 'Available'
  });

  // Modal xem chi tiết
  const [viewingRoom, setViewingRoom] = useState(null);

  // Modal xác nhận xóa
  const [deletingRoomId, setDeletingRoomId] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const fetchRooms = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await apiCall('/Rooms', { method: 'GET' });
      const data = Array.isArray(response) ? response : (response.value || response.data || []);
      setRooms(data);
    } catch (err) {
      console.error('Lỗi tải danh sách phòng:', err);
      setError(err.message || 'Không thể tải danh sách phòng họp.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchRooms();
  }, [fetchRooms]);

  const handleCreateSubmit = async (e) => {
    e.preventDefault();
    if (!newRoom.roomName.trim()) {
      alert('Vui lòng nhập tên phòng họp.');
      return;
    }

    setCreating(true);
    try {
      await apiCall('/Rooms', {
        method: 'POST',
        body: JSON.stringify({
          ...newRoom,
          capacity: Number(newRoom.capacity) || 20
        })
      });
      setShowCreateModal(false);
      setNewRoom({ roomName: '', location: '', capacity: 20, equipment: '', status: 'Available' });
      setSuccessMsg('Thêm phòng họp mới thành công!');
      setTimeout(() => setSuccessMsg(''), 4000);
      await fetchRooms();
    } catch (err) {
      alert(err.message || 'Không thể tạo phòng họp.');
    } finally {
      setCreating(false);
    }
  };

  const handleDeleteRoom = async () => {
    if (!deletingRoomId) return;
    setDeleting(true);
    try {
      await apiCall(`/Rooms/${deletingRoomId}`, { method: 'DELETE' });
      setDeletingRoomId(null);
      setSuccessMsg('Đã xóa phòng họp thành công!');
      setTimeout(() => setSuccessMsg(''), 4000);
      await fetchRooms();
    } catch (err) {
      alert(err.message || 'Không thể xóa phòng họp (có thể đang có cuộc họp diễn ra).');
    } finally {
      setDeleting(false);
    }
  };

  const getStatusBadge = (status) => {
    switch (status) {
      case 'Available':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-green-100 text-green-700">Sẵn sàng</span>;
      case 'In Use':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-blue-100 text-blue-700">Đang họp</span>;
      case 'Maintenance':
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-orange-100 text-orange-700">Bảo trì</span>;
      default:
        return <span className="px-3 py-1 rounded-full text-xs font-semibold bg-gray-100 text-gray-700">{status}</span>;
    }
  };

  return (
    <div className="p-6">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-800">Quản Lý Phòng Họp Vật Lý</h1>
          <p className="text-sm text-gray-500 mt-1">Danh mục phòng họp vật lý, sức chứa và thiết bị hội trường</p>
        </div>
        <button
          onClick={() => setShowCreateModal(true)}
          className="bg-red-600 hover:bg-red-700 text-white px-5 py-2.5 rounded-lg font-semibold flex items-center gap-2 shadow-sm transition-colors"
        >
          <Plus className="w-5 h-5" />
          Thêm Phòng Mới
        </button>
      </div>

      {/* Thông báo thành công */}
      {successMsg && (
        <div className="mb-4 bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded-lg flex items-center gap-2 text-sm">
          <CheckCircle2 className="w-5 h-5 text-green-600 shrink-0" />
          <span>{successMsg}</span>
        </div>
      )}

      {/* Thông báo lỗi */}
      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg flex items-center gap-2 text-sm">
          <AlertCircle className="w-5 h-5 text-red-600 shrink-0" />
          <span>{error}</span>
          <button onClick={fetchRooms} className="ml-auto underline font-medium">Thử lại</button>
        </div>
      )}

      {/* Bảng dữ liệu */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
        {loading ? (
          <div className="py-20 text-center text-gray-500 flex flex-col items-center justify-center gap-3">
            <Loader2 className="w-8 h-8 animate-spin text-red-600" />
            <p className="text-sm font-medium">Đang tải danh sách phòng họp...</p>
          </div>
        ) : rooms.length === 0 ? (
          <div className="py-16 text-center text-gray-500">
            <p className="text-base font-medium">Chưa có phòng họp nào trong hệ thống.</p>
            <button
              onClick={() => setShowCreateModal(true)}
              className="mt-3 text-red-600 hover:underline text-sm font-semibold"
            >
              + Tạo phòng họp đầu tiên
            </button>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead className="bg-gray-50 border-b border-gray-200 text-xs font-semibold text-gray-600 uppercase">
                <tr>
                  <th className="px-6 py-3.5">Tên Phòng</th>
                  <th className="px-6 py-3.5">Vị Trí</th>
                  <th className="px-6 py-3.5 text-center">Sức Chứa</th>
                  <th className="px-6 py-3.5">Trạng Thái</th>
                  <th className="px-6 py-3.5">Người Đang Dùng</th>
                  <th className="px-6 py-3.5">Lịch Sắp Tới</th>
                  <th className="px-6 py-3.5 text-right">Thao Tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 text-sm">
                {rooms.map((room, index) => (
                  <tr key={room.roomId} className={index % 2 === 0 ? 'bg-white hover:bg-gray-50/80' : 'bg-gray-50/40 hover:bg-gray-50/80'}>
                    <td className="px-6 py-4 font-semibold text-gray-800">{room.roomName}</td>
                    <td className="px-6 py-4 text-gray-600">{room.location || '-'}</td>
                    <td className="px-6 py-4 text-center text-gray-700 font-medium">{room.capacity} người</td>
                    <td className="px-6 py-4">{getStatusBadge(room.status)}</td>
                    <td className="px-6 py-4 text-gray-700">{room.currentUser || '-'}</td>
                    <td className="px-6 py-4 text-gray-500 text-xs">{room.upcomingSchedule || '-'}</td>
                    <td className="px-6 py-4 text-right">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => setViewingRoom(room)}
                          className="p-1.5 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                          title="Xem chi tiết thiết bị"
                        >
                          <Eye className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => setDeletingRoomId(room.roomId)}
                          className="p-1.5 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                          title="Xóa phòng"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal Tạo Phòng Mới */}
      {showCreateModal && (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg overflow-hidden border border-gray-100">
            <div className="flex justify-between items-center px-6 py-4 border-b border-gray-100 bg-gray-50/60">
              <h2 className="text-lg font-bold text-gray-800">Thêm Phòng Họp Mới</h2>
              <button
                onClick={() => setShowCreateModal(false)}
                className="p-1 text-gray-400 hover:text-gray-600 rounded-lg"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            <form onSubmit={handleCreateSubmit} className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-1">Tên phòng họp *</label>
                <input
                  type="text"
                  required
                  placeholder="Ví dụ: Hội trường Đa năng A1"
                  value={newRoom.roomName}
                  onChange={(e) => setNewRoom({ ...newRoom, roomName: e.target.value })}
                  className="w-full px-3.5 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-red-500 focus:border-red-500 outline-none"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-1">Vị trí / Tòa nhà</label>
                  <input
                    type="text"
                    placeholder="Tòa nhà C2 - Tầng 3"
                    value={newRoom.location}
                    onChange={(e) => setNewRoom({ ...newRoom, location: e.target.value })}
                    className="w-full px-3.5 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-red-500 focus:border-red-500 outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-1">Sức chứa (người)</label>
                  <input
                    type="number"
                    min="1"
                    max="1000"
                    value={newRoom.capacity}
                    onChange={(e) => setNewRoom({ ...newRoom, capacity: e.target.value })}
                    className="w-full px-3.5 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-red-500 focus:border-red-500 outline-none"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-1">Trang thiết bị</label>
                <textarea
                  rows="2"
                  placeholder="Màn hình LED 200 inch, Hệ thống Micro đa hướng, Thiết bị hội nghị 4K..."
                  value={newRoom.equipment}
                  onChange={(e) => setNewRoom({ ...newRoom, equipment: e.target.value })}
                  className="w-full px-3.5 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-red-500 focus:border-red-500 outline-none resize-none"
                />
              </div>

              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-1">Trạng thái ban đầu</label>
                <select
                  value={newRoom.status}
                  onChange={(e) => setNewRoom({ ...newRoom, status: e.target.value })}
                  className="w-full px-3.5 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-red-500 focus:border-red-500 outline-none"
                >
                  <option value="Available">Sẵn sàng sử dụng</option>
                  <option value="Maintenance">Đang bảo trì</option>
                </select>
              </div>

              <div className="flex justify-end gap-3 pt-4 border-t border-gray-100">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-50"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  disabled={creating}
                  className="px-5 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm font-semibold transition-colors disabled:opacity-50"
                >
                  {creating ? 'Đang tạo...' : 'Tạo Phòng'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Xem Chi Tiết Phòng */}
      {viewingRoom && (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
            <div className="flex justify-between items-center mb-4 pb-2 border-b">
              <h3 className="text-lg font-bold text-gray-800">{viewingRoom.roomName}</h3>
              <button onClick={() => setViewingRoom(null)} className="text-gray-400 hover:text-gray-600">
                <X className="w-5 h-5" />
              </button>
            </div>
            <div className="space-y-3 text-sm">
              <div>
                <span className="text-gray-500 block">Vị trí:</span>
                <span className="font-semibold text-gray-800">{viewingRoom.location || 'Chưa cập nhật'}</span>
              </div>
              <div>
                <span className="text-gray-500 block">Sức chứa tối đa:</span>
                <span className="font-semibold text-gray-800">{viewingRoom.capacity} người</span>
              </div>
              <div>
                <span className="text-gray-500 block">Trang thiết bị sẵn có:</span>
                <p className="font-medium text-gray-700 bg-gray-50 p-3 rounded-lg mt-1 border border-gray-100">
                  {viewingRoom.equipment || 'Chưa có thông tin thiết bị.'}
                </p>
              </div>
              <div>
                <span className="text-gray-500 block">Trạng thái:</span>
                <div className="mt-1">{getStatusBadge(viewingRoom.status)}</div>
              </div>
            </div>
            <button
              onClick={() => setViewingRoom(null)}
              className="mt-6 w-full py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium rounded-lg text-sm"
            >
              Đóng
            </button>
          </div>
        </div>
      )}

      {/* Modal Xác Nhận Xóa */}
      {deletingRoomId && (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-6 text-center">
            <div className="w-12 h-12 bg-red-100 text-red-600 rounded-full flex items-center justify-center mx-auto mb-3">
              <Trash2 className="w-6 h-6" />
            </div>
            <h3 className="text-lg font-bold text-gray-800 mb-2">Xác nhận xóa phòng họp?</h3>
            <p className="text-sm text-gray-500 mb-6">
              Hành động này không thể hoàn tác. Bạn chỉ có thể xóa phòng họp nếu không có cuộc họp nào đang hoặc sắp diễn ra.
            </p>
            <div className="flex gap-3 justify-center">
              <button
                onClick={() => setDeletingRoomId(null)}
                className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-50"
              >
                Hủy
              </button>
              <button
                onClick={handleDeleteRoom}
                disabled={deleting}
                className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm font-semibold disabled:opacity-50"
              >
                {deleting ? 'Đang xóa...' : 'Xóa Phòng'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default RoomsPage;
