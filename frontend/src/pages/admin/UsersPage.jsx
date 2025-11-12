import React, { useState } from 'react';
import { Edit, Lock, Unlock, Eye } from 'lucide-react';

const UsersPage = () => {
  const [users] = useState([
    { id: 1, name: 'Nguyễn Văn A', email: 'nguyenvana@example.com', position: 'Manager', role: 'Admin', status: 'Active' },
    { id: 2, name: 'Trần Thị B', email: 'tranthib@example.com', position: 'Staff', role: 'User', status: 'Active' },
    { id: 3, name: 'Lê Văn C', email: 'levanc@example.com', position: 'Staff', role: 'User', status: 'Locked' }
  ]);

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold text-text mb-6">Users</h1>

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
                      <button className="p-2 text-primary hover:bg-secondary rounded-button transition-colors" title="Edit">
                        <Edit className="w-4 h-4" />
                      </button>
                      {user.status === 'Active' ? (
                        <button className="p-2 text-orange-600 hover:bg-secondary rounded-button transition-colors" title="Lock">
                          <Lock className="w-4 h-4" />
                        </button>
                      ) : (
                        <button className="p-2 text-green-600 hover:bg-secondary rounded-button transition-colors" title="Unlock">
                          <Unlock className="w-4 h-4" />
                        </button>
                      )}
                      <button className="p-2 text-primary hover:bg-secondary rounded-button transition-colors" title="View History">
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
    </div>
  );
};

export default UsersPage;

