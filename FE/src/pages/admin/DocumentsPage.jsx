import React, { useState, useEffect, useCallback } from 'react';
import { Eye, FileText, Download, Loader2, AlertCircle, FileCheck, ShieldAlert } from 'lucide-react';
import { apiCall } from '../../utils/api';
import InAppDocumentViewer from '../../components/InAppDocumentViewer';

const DocumentsPage = () => {
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [viewingDoc, setViewingDoc] = useState(null); // { id, name }

  const fetchDocuments = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await apiCall('/Document', { method: 'GET' });
      const data = Array.isArray(response) ? response : (response.value || response.data || []);
      setDocuments(data);
    } catch (err) {
      console.error('Lỗi nạp danh sách tài liệu:', err);
      setError(err.message || 'Không thể tải danh sách tài liệu.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchDocuments();
  }, [fetchDocuments]);

  const handleDownload = async (doc) => {
    try {
      const token = localStorage.getItem('token');
      const baseUrl = import.meta.env.VITE_API_URL || '/api';
      const response = await fetch(`${baseUrl}/Document/${doc.documentId}/download`, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });

      if (!response.ok) {
        throw new Error('Bạn không có quyền tải file này hoặc file không tồn tại.');
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = doc.fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (err) {
      alert(err.message || 'Không thể tải tài liệu.');
    }
  };

  const formatDate = (dateStr) => {
    if (!dateStr) return '-';
    try {
      const d = new Date(dateStr);
      return d.toLocaleDateString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return dateStr;
    }
  };

  const getVisibilityBadge = (visibility) => {
    switch (visibility) {
      case 'Chung':
        return <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-green-100 text-green-700">Công khai</span>;
      case 'NoiBo':
        return <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-100 text-blue-700">Nội bộ cuộc họp</span>;
      case 'Mat':
        return <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-red-100 text-red-700 flex items-center gap-1"><ShieldAlert className="w-3 h-3" /> Mật</span>;
      default:
        return <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-gray-100 text-gray-700">{visibility}</span>;
    }
  };

  return (
    <div className="p-6">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-800">Kho Tài Liệu Cuộc Họp</h1>
          <p className="text-sm text-gray-500 mt-1">Toàn bộ văn bản, tờ trình và tài liệu số hóa trong hệ thống</p>
        </div>
      </div>

      {/* Thông báo lỗi */}
      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg flex items-center gap-2 text-sm">
          <AlertCircle className="w-5 h-5 text-red-600 shrink-0" />
          <span>{error}</span>
          <button onClick={fetchDocuments} className="ml-auto underline font-medium">Thử lại</button>
        </div>
      )}

      {/* Bảng dữ liệu */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
        {loading ? (
          <div className="py-20 text-center text-gray-500 flex flex-col items-center justify-center gap-3">
            <Loader2 className="w-8 h-8 animate-spin text-red-600" />
            <p className="text-sm font-medium">Đang tải danh mục tài liệu số...</p>
          </div>
        ) : documents.length === 0 ? (
          <div className="py-16 text-center text-gray-500">
            <FileCheck className="w-12 h-12 text-gray-400 mx-auto mb-3" />
            <p className="text-base font-medium">Chưa có tài liệu nào được tải lên hệ thống.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead className="bg-gray-50 border-b border-gray-200 text-xs font-semibold text-gray-600 uppercase">
                <tr>
                  <th className="px-6 py-3.5">Tên Văn Bản</th>
                  <th className="px-6 py-3.5">Cuộc Họp Liên Quan</th>
                  <th className="px-6 py-3.5">Người Tải Lên</th>
                  <th className="px-6 py-3.5">Thời Gian</th>
                  <th className="px-6 py-3.5">Phân Loại</th>
                  <th className="px-6 py-3.5 text-right">Thao Tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 text-sm">
                {documents.map((doc, index) => (
                  <tr key={doc.documentId} className={index % 2 === 0 ? 'bg-white hover:bg-gray-50/80' : 'bg-gray-50/40 hover:bg-gray-50/80'}>
                    <td className="px-6 py-4 font-semibold text-gray-800 flex items-center gap-2.5">
                      <FileText className="w-4 h-4 text-red-600 shrink-0" />
                      <span className="truncate max-w-xs" title={doc.fileName}>{doc.fileName}</span>
                    </td>
                    <td className="px-6 py-4 text-gray-700">{doc.meetingTitle || `Cuộc họp #${doc.meetingId}`}</td>
                    <td className="px-6 py-4 text-gray-600">{doc.uploaderName || `User #${doc.uploadedBy}`}</td>
                    <td className="px-6 py-4 text-gray-500 text-xs">{formatDate(doc.uploadDate)}</td>
                    <td className="px-6 py-4">{getVisibilityBadge(doc.visibility)}</td>
                    <td className="px-6 py-4 text-right">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => setViewingDoc({ id: doc.documentId, name: doc.fileName })}
                          className="px-2.5 py-1.5 text-xs font-medium text-blue-600 bg-blue-50 hover:bg-blue-100 rounded-lg transition-colors flex items-center gap-1"
                          title="Xem trực tiếp trong ứng dụng"
                        >
                          <Eye className="w-3.5 h-3.5" />
                          <span>Xem</span>
                        </button>
                        <button
                          onClick={() => handleDownload(doc)}
                          className="p-1.5 text-gray-600 hover:text-red-600 hover:bg-gray-100 rounded-lg transition-colors"
                          title="Tải về máy"
                        >
                          <Download className="w-4 h-4" />
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

      {/* Trình Xem Tài Liệu Nhúng In-App */}
      {viewingDoc && (
        <InAppDocumentViewer
          documentId={viewingDoc.id}
          fileName={viewingDoc.name}
          onClose={() => setViewingDoc(null)}
        />
      )}
    </div>
  );
};

export default DocumentsPage;
