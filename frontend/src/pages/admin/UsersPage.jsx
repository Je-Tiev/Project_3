import React, { useState } from 'react';
import { Edit, Lock, Unlock, Eye, Plus, CheckCircle, XCircle } from 'lucide-react';
import { apiCall } from '../../utils/api'; 

const UsersPage = () => {
  const [users, setUsers] = useState([
    { id: 1, name: 'Nguyễn Văn A', email: 'nguyenvana@example.com', position: 'Manager', role: 'Admin', status: 'Active' },
    { id: 2, name: 'Trần Thị B', email: 'tranthib@example.com', position: 'Staff', role: 'User', status: 'Active' },
    { id: 3, name: 'Lê Văn C', email: 'levanc@example.com', position: 'Staff', role: 'User', status: 'Locked' }
  ]);

  const [showForm, setShowForm] = useState(false);
  const [newUser, setNewUser] = useState({
    fullName: "",
    username: "",
    email: "",
    password: "",
    departmentID: 1
  });
  // Thêm state để quản lý thông báo
  const [feedback, setFeedback] = useState({ message: '', type: '' }); 

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    const payload = {
      fullName: newUser.fullName,
      username: newUser.username,
      email: newUser.email,
      password: newUser.password,
      departmentID: newUser.departmentID
    };
    console.log("Payload gửi lên:", payload);
    
    // Xóa thông báo cũ
    setFeedback({ message: 'Đang xử lý...', type: 'info' }); 

    try {
        const responseData = await apiCall("/Auth/register", { 
            method: "POST",
            body: JSON.stringify(payload)
        });
        
      console.log("Response từ API:", responseData);
      
      // Lấy thông báo từ phản hồi (có thể là JSON object có .message hoặc chuỗi text)
      const successMessage = typeof responseData === 'object' && responseData.message 
                           ? responseData.message 
                           : responseData.toString() || 'Đăng ký người dùng thành công.';

      // Cập nhật thông báo thành công và đóng form
      setFeedback({ message: successMessage, type: 'success' });
      
      // Thêm người dùng mới vào danh sách và hiển thị ngay lập tức
      setUsers([
         ...users,
         {
          id: Date.now(), // ID tạm thời độc nhất hơn
          name: newUser.fullName,
          email: newUser.email,
          position: "Staff", 
          role: "User", 
          status: "Active" 
        }]);
        
        setShowForm(false);
        setNewUser({ fullName: "", username: "", email: "", password: "", departmentID: 1 });
      } catch (err) {
        console.error("Đăng ký thất bại:", err);
        
        // Cập nhật thông báo lỗi
        setFeedback({ message: `Đăng ký thất bại! Lỗi: ${err.message}`, type: 'error' });
      }
      
      // Tự động xóa thông báo sau 5 giây (tùy chọn)
      setTimeout(() => setFeedback({ message: '', type: '' }), 5000); 
  };

  return (
    <div className="p-4"> 

      {/* HIỂN THỊ THÔNG BÁO CHUNG NGAY TRÊN TABLE */}
      {feedback.message && (
        <div className={`p-4 mb-4 rounded-lg flex items-center gap-2 ${
          feedback.type === 'success' ? 'bg-green-100 text-green-700' : 
          feedback.type === 'error' ? 'bg-red-100 text-red-700' : 'bg-blue-100 text-blue-700'
        }`}>
          {feedback.type === 'success' ? <CheckCircle className="w-5 h-5" /> : <XCircle className="w-5 h-5" />}
          <p className="font-medium">{feedback.message}</p>
        </div>
      )}
      
        {/* NÚT THÊM NGƯỜI DÙNG MỚI */}
        <div className="mb-4 flex justify-end">
            <button 
                onClick={() => setShowForm(true)}
                className="flex items-center gap-1 bg-primary text-white px-4 py-2 rounded-lg shadow-md hover:bg-primary-dark transition duration-150"
            >
                <Plus className="w-5 h-5" /> Add User
            </button>
        </div>
        
        {/* TABLE (Không thay đổi) */}
        {/* ... (Giữ nguyên phần Table) */}
        <div className="bg-white border border-secondary-dark rounded-lg shadow-sm overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-secondary">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Name</th>
                  <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Email</th>
                  <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Position</th>
                  <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Role</th>
                  <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Status</th>
                  <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-secondary-dark">
                {users.map((user, index) => (
                  <tr key={user.id} className={index % 2 === 0 ? 'bg-white' : 'bg-secondary'}>
                    <td className="px-6 py-4 text-sm text-text font-medium">{user.name}</td>
                    <td className="px-6 py-4 text-sm text-text">{user.email}</td>
                    <td className="px-6 py-4 text-sm text-text">{user.position}</td>
                    <td className="px-6 py-4 text-sm">
                      <span className={`px-3 py-1 rounded-full text-xs font-semibold ${
                        user.role === 'Admin' ? 'bg-primary text-white' : 'bg-secondary text-text'
                      }`}>
                        {user.role}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm">
                      <span className={`px-3 py-1 rounded-full text-xs font-semibold ${
                        user.status === 'Active' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                      }`}>
                        {user.status}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        <button className="p-2 text-primary hover:bg-secondary rounded-button" title="Edit">
                          <Edit className="w-4 h-4" />
                        </button>
                        {user.status === 'Active' ? (
                          <button className="p-2 text-orange-600 hover:bg-secondary rounded-button" title="Lock">
                            <Lock className="w-4 h-4" />
                          </button>
                        ) : (
                          <button className="p-2 text-green-600 hover:bg-secondary rounded-button" title="Unlock">
                            <Unlock className="w-4 h-4" />
                          </button>
                        )}
                        <button className="p-2 text-primary hover:bg-secondary rounded-button" title="View History">
                          <Eye className="w-4 h-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* MODAL FORM (Không thay đổi) */}
        {showForm && (
          <div className="fixed inset-0 bg-black bg-opacity-40 flex justify-center items-center z-50"> 
            <div className="bg-white p-6 rounded-lg w-96 shadow-xl">
              <h2 className="text-xl font-bold mb-4">Add New User</h2>

              <form onSubmit={handleSubmit} className="space-y-3">
                {/* HIỂN THỊ THÔNG BÁO BÊN TRONG MODAL (chỉ khi đang mở form) */}
                {feedback.message && feedback.type !== 'success' && (
                  <div className={`p-2 rounded-lg flex items-center gap-2 ${
                    feedback.type === 'error' ? 'bg-red-100 text-red-700' : 'bg-blue-100 text-blue-700'
                  }`}>
                    <XCircle className="w-4 h-4" />
                    <p className="text-sm font-medium">{feedback.message}</p>
                  </div>
                )}
                {/* ... (Giữ nguyên các Input) */}
                <input
                  className="w-full border p-2 rounded"
                  placeholder="Full Name"
                  value={newUser.fullName}
                  onChange={(e) => setNewUser({ ...newUser, fullName: e.target.value })}
                  required
                />

                <input
                  className="w-full border p-2 rounded"
                  placeholder="Username"
                  value={newUser.username}
                  onChange={(e) => setNewUser({ ...newUser, username: e.target.value })}
                  required
                />

                <input
                  className="w-full border p-2 rounded"
                  placeholder="Email"
                  value={newUser.email}
                  onChange={(e) => setNewUser({ ...newUser, email: e.target.value })}
                  required
                />

                <input
                  type="password"
                  className="w-full border p-2 rounded"
                  placeholder="Password"
                  value={newUser.password}
                  onChange={(e) => setNewUser({ ...newUser, password: e.target.value })}
                  required
                />

                <button
                  type="submit"
                  className="w-full bg-primary text-white p-2 rounded hover:bg-primary-dark"
                >
                  Create User
                </button>

                <button
                  type="button"
                  className="w-full bg-gray-300 mt-2 p-2 rounded hover:bg-gray-400"
                  onClick={() => setShowForm(false)}
                >
                  Cancel
                </button>

              </form>
            </div>
          </div>
        )}
    </div>
  );
};

export default UsersPage;