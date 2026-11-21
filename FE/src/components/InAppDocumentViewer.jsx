import React, { useState, useEffect } from 'react';
import { X, Download, Maximize2, Minimize2, Loader2, AlertCircle, FileText, ChevronLeft, ChevronRight, Radio } from 'lucide-react';
import api from '../utils/api';

const InAppDocumentViewer = ({
  documentId,
  fileName,
  onClose,
  isPresenter = false,
  onBroadcastSlideChange = null,
  syncedPage = null,
}) => {
  const [blobUrl, setBlobUrl] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);

  // When syncedPage arrives from SignalR and attendee is watching
  useEffect(() => {
    if (syncedPage && !isPresenter) {
      setCurrentPage(syncedPage);
    }
  }, [syncedPage, isPresenter]);

  useEffect(() => {
    let active = true;
    const fetchDocumentBlob = async () => {
      setLoading(true);
      setError('');
      try {
        const token = localStorage.getItem('token');
        const baseUrl = import.meta.env.VITE_API_URL || '/api';

        const response = await fetch(`${baseUrl}/Document/${documentId}/view`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        if (!response.ok) {
          if (response.status === 403) {
            throw new Error('Bạn không có quyền truy cập văn bản mật này.');
          }
          throw new Error(`Lỗi tải văn bản: HTTP ${response.status}`);
        }

        const blob = await response.blob();
        if (active) {
          const url = window.URL.createObjectURL(blob);
          setBlobUrl(url);
        }
      } catch (err) {
        console.error('Lỗi khi nạp văn bản:', err);
        if (active) {
          setError(err.message || 'Không thể hiển thị tài liệu.');
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    };

    if (documentId) {
      fetchDocumentBlob();
    }

    return () => {
      active = false;
      if (blobUrl) {
        window.URL.revokeObjectURL(blobUrl);
      }
    };
  }, [documentId]);

  const handleDownload = () => {
    if (!blobUrl) return;
    const link = document.createElement('a');
    link.href = blobUrl;
    link.download = fileName || 'document.pdf';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const changePage = (newPage) => {
    const p = Math.max(1, newPage);
    setCurrentPage(p);
    if (isPresenter && onBroadcastSlideChange) {
      onBroadcastSlideChange({
        documentId: Number(documentId),
        pageNumber: p,
        zoomLevel: 1.0,
      });
    }
  };

  const isPdf = fileName?.toLowerCase().endsWith('.pdf');

  return (
    <div className="fixed inset-0 z-50 bg-black/75 backdrop-blur-xs flex items-center justify-center p-3 sm:p-6">
      <div
        className={`bg-white rounded-2xl shadow-2xl flex flex-col overflow-hidden transition-all duration-200 border border-gray-200 ${
          isFullscreen ? 'w-full h-full rounded-none' : 'w-full max-w-5xl h-[88vh]'
        }`}
      >
        {/* Header Bar */}
        <div className="flex justify-between items-center px-6 py-3.5 bg-gray-900 text-white border-b border-gray-800">
          <div className="flex items-center gap-3 overflow-hidden">
            <FileText className="w-5 h-5 text-red-500 shrink-0" />
            <span className="font-semibold text-sm truncate" title={fileName}>
              {fileName || 'Tài Liệu Đính Kèm'}
            </span>
            <span className="text-xs bg-gray-800 px-2 py-0.5 rounded text-gray-400 font-mono">
              In-App Viewer
            </span>
            {isPresenter ? (
              <span className="hidden sm:flex items-center gap-1 text-[11px] bg-red-900/80 text-red-300 px-2 py-0.5 rounded-full font-medium">
                <Radio size={12} className="animate-pulse" /> Đang phát sóng slide
              </span>
            ) : syncedPage ? (
              <span className="hidden sm:flex items-center gap-1 text-[11px] bg-blue-900/80 text-blue-300 px-2 py-0.5 rounded-full font-medium">
                Đồng bộ trang {syncedPage}
              </span>
            ) : null}
          </div>

          <div className="flex items-center gap-2">
            {/* Slide Navigation for Presenter */}
            {isPdf && (
              <div className="flex items-center gap-1 bg-gray-800 px-2 py-1 rounded-lg text-xs mr-2">
                <button
                  onClick={() => changePage(currentPage - 1)}
                  disabled={currentPage <= 1}
                  className="p-1 hover:bg-gray-700 disabled:opacity-30 rounded text-gray-300"
                  title="Trang trước"
                >
                  <ChevronLeft size={16} />
                </button>
                <span className="px-1.5 font-mono text-gray-200">Trang {currentPage}</span>
                <button
                  onClick={() => changePage(currentPage + 1)}
                  className="p-1 hover:bg-gray-700 rounded text-gray-300"
                  title="Trang sau"
                >
                  <ChevronRight size={16} />
                </button>
              </div>
            )}

            <button
              onClick={handleDownload}
              className="p-1.5 text-gray-300 hover:text-white hover:bg-gray-800 rounded-lg transition-colors text-xs flex items-center gap-1.5 px-3"
              title="Tải về máy"
            >
              <Download className="w-4 h-4" />
              <span className="hidden sm:inline">Tải về</span>
            </button>
            <button
              onClick={() => setIsFullscreen(!isFullscreen)}
              className="p-1.5 text-gray-300 hover:text-white hover:bg-gray-800 rounded-lg transition-colors"
              title={isFullscreen ? 'Thu nhỏ' : 'Toàn màn hình'}
            >
              {isFullscreen ? <Minimize2 className="w-4 h-4" /> : <Maximize2 className="w-4 h-4" />}
            </button>
            <button
              onClick={onClose}
              className="p-1.5 text-gray-300 hover:text-red-400 hover:bg-gray-800 rounded-lg transition-colors"
              title="Đóng"
            >
              <X className="w-5 h-5" />
            </button>
          </div>
        </div>

        {/* Content Body */}
        <div className="flex-1 bg-gray-100 relative overflow-hidden flex items-center justify-center">
          {loading && (
            <div className="flex flex-col items-center gap-3 text-gray-600">
              <Loader2 className="w-10 h-10 animate-spin text-red-600" />
              <p className="text-sm font-medium">Đang tải và giải mã văn bản số...</p>
            </div>
          )}

          {error && (
            <div className="text-center p-8 max-w-md">
              <div className="w-12 h-12 bg-red-100 text-red-600 rounded-full flex items-center justify-center mx-auto mb-3">
                <AlertCircle className="w-6 h-6" />
              </div>
              <h3 className="text-base font-bold text-gray-800 mb-1">Không thể mở tài liệu</h3>
              <p className="text-sm text-gray-600 mb-4">{error}</p>
              <button
                onClick={onClose}
                className="px-4 py-2 bg-gray-200 hover:bg-gray-300 text-gray-700 rounded-lg text-sm font-medium"
              >
                Đóng cửa sổ
              </button>
            </div>
          )}

          {!loading && !error && blobUrl && (
            isPdf ? (
              <iframe
                key={`page-${currentPage}`}
                src={`${blobUrl}#page=${currentPage}&toolbar=1&navpanes=1`}
                className="w-full h-full border-0"
                title={fileName}
              />
            ) : (
              <div className="text-center p-8">
                <FileText className="w-16 h-16 text-blue-500 mx-auto mb-4" />
                <h3 className="text-lg font-bold text-gray-800 mb-2">Định dạng xem thử</h3>
                <p className="text-sm text-gray-600 mb-6">
                  Tài liệu định dạng văn bản Office. Vui lòng tải về máy để xem với ứng dụng chuyên dụng.
                </p>
                <button
                  onClick={handleDownload}
                  className="px-5 py-2.5 bg-red-600 hover:bg-red-700 text-white rounded-lg text-sm font-semibold flex items-center gap-2 mx-auto shadow-sm"
                >
                  <Download className="w-4 h-4" />
                  Tải Về Máy
                </button>
              </div>
            )
          )}
        </div>
      </div>
    </div>
  );
};

export default InAppDocumentViewer;
