import React, { useState, useEffect } from 'react';
import { FileText, Eye, Download, File, Lock, Shield } from 'lucide-react';
import { apiCall } from '../../utils/api';
import InAppDocumentViewer from '../InAppDocumentViewer';

export default function MeetingDocsPanel({ meetingId }) {
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [viewingDoc, setViewingDoc] = useState(null);

  const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

  useEffect(() => {
    let isMounted = true;
    const fetchDocs = async () => {
      try {
        setLoading(true);
        const res = await apiCall(`/Document/GetDocumentsByMeeting/meeting/${meetingId}`, {
          method: 'GET',
        });
        if (isMounted && Array.isArray(res)) {
          setDocuments(res);
        }
      } catch (err) {
        console.error('Lỗi lấy tài liệu cuộc họp:', err);
      } finally {
        if (isMounted) setLoading(false);
      }
    };

    if (meetingId) {
      fetchDocs();
    }

    return () => {
      isMounted = false;
    };
  }, [meetingId]);

  const handleDownload = async (docId, fileName) => {
    try {
      const token = localStorage.getItem('token');
      const res = await fetch(`${API_BASE_URL}/Document/DownloadDocument/${docId}/download`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error('Không thể tải file');
      const blob = await res.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (err) {
      alert('Lỗi tải tài liệu: ' + err.message);
    }
  };

  const getVisibilityBadge = (vis) => {
    if (vis === 2 || vis === 'Confidential' || vis === 'Mật') {
      return (
        <span className="flex items-center gap-1 text-[10px] bg-red-900/80 text-red-300 px-2 py-0.5 rounded-full font-medium">
          <Lock size={10} /> Mật
        </span>
      );
    }
    if (vis === 1 || vis === 'Internal' || vis === 'Nội bộ') {
      return (
        <span className="flex items-center gap-1 text-[10px] bg-amber-900/80 text-amber-300 px-2 py-0.5 rounded-full font-medium">
          <Shield size={10} /> Nội bộ
        </span>
      );
    }
    return (
      <span className="text-[10px] bg-blue-900/80 text-blue-300 px-2 py-0.5 rounded-full font-medium">
        Công khai
      </span>
    );
  };

  return (
    <div className="flex flex-col h-full bg-gray-800 text-white border-l border-gray-700">
      {/* Header */}
      <div className="p-4 border-b border-gray-700 flex items-center justify-between">
        <div className="flex items-center gap-2 font-semibold">
          <FileText size={18} className="text-red-500" />
          <span>Tài liệu phiên họp ({documents.length})</span>
        </div>
      </div>

      {/* Documents List */}
      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        {loading && documents.length === 0 ? (
          <div className="text-center text-gray-400 py-8 text-sm">Đang tải tài liệu...</div>
        ) : documents.length === 0 ? (
          <div className="text-center text-gray-400 py-8 text-sm">
            Chưa có tài liệu số nào được tải lên cho phiên họp này.
          </div>
        ) : (
          documents.map((doc) => (
            <div
              key={doc.documentId}
              className="bg-gray-750 p-3.5 rounded-xl border border-gray-700 space-y-2 hover:border-gray-600 transition-colors"
            >
              <div className="flex items-start justify-between gap-2">
                <div className="flex items-start gap-2.5 min-w-0">
                  <File className="text-red-400 w-5 h-5 shrink-0 mt-0.5" />
                  <div className="min-w-0">
                    <p className="text-xs font-medium text-gray-100 truncate" title={doc.fileName}>
                      {doc.fileName}
                    </p>
                    <div className="flex items-center gap-2 mt-1">
                      {getVisibilityBadge(doc.visibility)}
                      {doc.uploadDate && (
                        <span className="text-[11px] text-gray-400">
                          {new Date(doc.uploadDate).toLocaleDateString('vi-VN')}
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              </div>

              {/* Actions */}
              <div className="flex justify-end gap-2 pt-1">
                <button
                  onClick={() => setViewingDoc({ id: doc.documentId, name: doc.fileName })}
                  className="flex items-center gap-1 px-2.5 py-1.5 bg-blue-600 hover:bg-blue-500 text-white rounded-lg text-xs font-medium transition-colors"
                >
                  <Eye size={13} /> Xem trực tiếp
                </button>
                <button
                  onClick={() => handleDownload(doc.documentId, doc.fileName)}
                  className="flex items-center gap-1 px-2.5 py-1.5 bg-gray-700 hover:bg-gray-600 text-gray-200 rounded-lg text-xs font-medium transition-colors"
                  title="Tải về máy"
                >
                  <Download size={13} /> Tải
                </button>
              </div>
            </div>
          ))
        )}
      </div>

      {/* In-App Document Viewer Modal */}
      {viewingDoc && (
        <InAppDocumentViewer
          documentId={viewingDoc.id}
          fileName={viewingDoc.name}
          onClose={() => setViewingDoc(null)}
        />
      )}
    </div>
  );
}
