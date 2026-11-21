import React, { useState, useEffect } from 'react';
import { FileCheck, PenTool, Printer, CheckCircle, Clock, ShieldCheck, Save } from 'lucide-react';
import { apiCall } from '../../utils/api';

export default function MeetingConclusionPanel({ meetingId, connection, isHostOrAdmin, currentUserId, meetingTitle }) {
  const [conclusion, setConclusion] = useState(null);
  const [summary, setSummary] = useState('');
  const [status, setStatus] = useState('Draft');
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [signing, setSigning] = useState(false);

  // Fetch conclusion
  const fetchConclusion = async () => {
    try {
      setLoading(true);
      const res = await apiCall(`/Conclusions/meeting/${meetingId}`, { method: 'GET' });
      if (res) {
        setConclusion(res);
        setSummary(res.summary || '');
        setStatus(res.status || 'Draft');
      }
    } catch (err) {
      console.warn('Lỗi lấy biên bản kết luận:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (meetingId) {
      fetchConclusion();
    }
  }, [meetingId]);

  // Hook into SignalR events
  useEffect(() => {
    if (!connection) return;

    const handleUpdated = (updated) => {
      setConclusion(updated);
      setSummary(updated.summary || '');
      setStatus(updated.status || 'Draft');
    };

    const handleSigned = (updated) => {
      setConclusion(updated);
    };

    connection.on('ConclusionUpdated', handleUpdated);
    connection.on('ConclusionSigned', handleSigned);

    return () => {
      connection.off('ConclusionUpdated', handleUpdated);
      connection.off('ConclusionSigned', handleSigned);
    };
  }, [connection]);

  // Handle Save (Host/Admin)
  const handleSaveConclusion = async (e) => {
    e.preventDefault();
    try {
      setSaving(true);
      const res = await apiCall(`/Conclusions/meeting/${meetingId}`, {
        method: 'POST',
        body: JSON.stringify({ summary, status }),
      });
      setConclusion(res);
      alert('Đã lưu dự thảo kết luận thành công!');
    } catch (err) {
      alert('Lỗi lưu kết luận: ' + (err.message || 'Lỗi hệ thống'));
    } finally {
      setSaving(false);
    }
  };

  // Handle Sign
  const handleSign = async () => {
    try {
      setSigning(true);
      const res = await apiCall(`/Conclusions/meeting/${meetingId}/sign`, { method: 'POST' });
      setConclusion(res);
    } catch (err) {
      alert('Lỗi ký xác nhận: ' + (err.message || 'Lỗi hệ thống'));
    } finally {
      setSigning(false);
    }
  };

  // Print Conclusion
  const handlePrint = () => {
    const printWindow = window.open('', '_blank');
    if (!printWindow) return;

    const signaturesHtml = (conclusion?.signatures || [])
      .map(
        (s) =>
          `<div style="display:inline-block; margin: 15px 30px; text-align:center;">
            <div style="font-style:italic; color:#059669; font-weight:bold;">[Đã ký điện tử]</div>
            <div style="font-weight:bold; margin-top:5px;">${s.fullName}</div>
            <div style="font-size:12px; color:#6b7280;">${new Date(s.signedAt).toLocaleString('vi-VN')}</div>
          </div>`
      )
      .join('');

    printWindow.document.write(`
      <html>
        <head>
          <title>Biên bản cuộc họp - ${meetingTitle || ''}</title>
          <style>
            body { font-family: 'Times New Roman', Times, serif; padding: 40px; color: #111; line-height: 1.6; }
            .header { text-align: center; margin-bottom: 30px; }
            .title { font-size: 20px; font-weight: bold; text-transform: uppercase; margin-top: 10px; }
            .meta { margin-bottom: 20px; font-size: 14px; }
            .content { white-space: pre-wrap; font-size: 15px; margin-bottom: 40px; }
            .signatures-title { font-weight: bold; margin-top: 30px; border-top: 1px solid #ccc; padding-top: 15px; }
          </style>
        </head>
        <body>
          <div class="header">
            <div>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div>
            <div><b>Độc lập - Tự do - Hạnh phúc</b></div>
            <hr style="width: 200px; margin: 8px auto;" />
            <div class="title">BIÊN BẢN KẾT LUẬN CUỘC HỌP</div>
            <div>${meetingTitle || ''}</div>
          </div>
          <div class="meta">
            <div><b>Thời gian lập:</b> ${conclusion?.createdAt ? new Date(conclusion.createdAt).toLocaleString('vi-VN') : ''}</div>
            <div><b>Trạng thái:</b> ${conclusion?.status === 'Finalized' ? 'Chính thức' : 'Dự thảo'}</div>
          </div>
          <h3>NỘI DUNG KẾT LUẬN & CHỈ ĐẠO:</h3>
          <div class="content">${conclusion?.summary || 'Chưa có nội dung kết luận.'}</div>
          <div class="signatures-title">DANH SÁCH ĐẠI BIỂU ĐÃ KÝ XÁC NHẬN:</div>
          <div style="text-align:center;">${signaturesHtml || '<i>Chưa có chữ ký nào.</i>'}</div>
        </body>
      </html>
    `);
    printWindow.document.close();
    printWindow.focus();
    setTimeout(() => {
      printWindow.print();
    }, 500);
  };

  const hasSigned = conclusion?.signatures?.some((s) => `${s.userId}` === `${currentUserId}`);

  return (
    <div className="flex flex-col h-full bg-gray-800 text-white border-l border-gray-700">
      {/* Header */}
      <div className="p-4 border-b border-gray-700 flex items-center justify-between">
        <div className="flex items-center gap-2 font-semibold">
          <FileCheck size={18} className="text-red-500" />
          <span>Biên bản & Kết luận</span>
        </div>
        <button
          onClick={handlePrint}
          className="flex items-center gap-1 text-xs bg-gray-700 hover:bg-gray-600 px-3 py-1.5 rounded-lg transition-colors text-gray-200"
          title="In hoặc xuất biên bản"
        >
          <Printer size={14} /> Xuất PDF
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {/* Status Badge */}
        <div className="flex items-center justify-between bg-gray-750 p-3 rounded-xl border border-gray-700">
          <div className="flex items-center gap-2">
            <span
              className={`px-2.5 py-1 rounded-full text-xs font-semibold ${
                status === 'Finalized'
                  ? 'bg-emerald-900/80 text-emerald-300'
                  : 'bg-amber-900/80 text-amber-300'
              }`}
            >
              {status === 'Finalized' ? 'Biên bản chính thức' : 'Dự thảo kết luận'}
            </span>
          </div>
          {conclusion?.creatorName && (
            <span className="text-xs text-gray-400">Soạn bởi: {conclusion.creatorName}</span>
          )}
        </div>

        {/* Content View / Edit */}
        {isHostOrAdmin ? (
          <form onSubmit={handleSaveConclusion} className="space-y-3">
            <label className="block text-xs font-medium text-gray-300">
              Nội dung kết luận cuộc họp (Chủ trì / Thư ký soạn thảo):
            </label>
            <textarea
              rows={8}
              value={summary}
              onChange={(e) => setSummary(e.target.value)}
              placeholder="Nhập các kết luận, quyết định và phân công nhiệm vụ của cuộc họp..."
              className="w-full bg-gray-750 border border-gray-700 rounded-xl p-3 text-sm text-gray-100 placeholder-gray-500 focus:outline-none focus:border-red-500 resize-none font-sans leading-relaxed"
            />
            <div className="flex items-center justify-between">
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                className="bg-gray-700 border border-gray-600 rounded-lg px-3 py-1.5 text-xs text-white"
              >
                <option value="Draft">Dự thảo</option>
                <option value="Finalized">Chính thức (Khóa)</option>
              </select>
              <button
                type="submit"
                disabled={saving}
                className="flex items-center gap-1.5 px-4 py-2 bg-red-600 hover:bg-red-700 disabled:opacity-50 text-white rounded-lg text-xs font-medium transition-colors"
              >
                <Save size={14} /> {saving ? 'Đang lưu...' : 'Lưu kết luận'}
              </button>
            </div>
          </form>
        ) : (
          <div className="bg-gray-750 p-4 rounded-xl border border-gray-700">
            <h4 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2">
              Nội dung kết luận:
            </h4>
            <div className="text-sm text-gray-200 whitespace-pre-wrap leading-relaxed">
              {summary || 'Chủ tọa đang dự thảo kết luận cuộc họp...'}
            </div>
          </div>
        )}

        {/* Digital Signatures Section */}
        <div className="bg-gray-750 p-4 rounded-xl border border-gray-700 space-y-3">
          <div className="flex items-center justify-between">
            <h4 className="text-xs font-semibold text-gray-300 uppercase tracking-wider flex items-center gap-1.5">
              <ShieldCheck size={14} className="text-emerald-400" /> Chữ ký xác nhận ({conclusion?.signatures?.length || 0})
            </h4>
            {!hasSigned ? (
              <button
                onClick={handleSign}
                disabled={signing || !conclusion?.conclusionId}
                className="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-500 disabled:opacity-50 text-white rounded-lg text-xs font-medium transition-colors shadow-md shadow-emerald-600/20"
              >
                <PenTool size={13} /> {signing ? 'Đang ký...' : 'Ký xác nhận'}
              </button>
            ) : (
              <span className="flex items-center gap-1 text-xs text-emerald-400 font-medium">
                <CheckCircle size={14} /> Bạn đã ký
              </span>
            )}
          </div>

          <div className="space-y-2 pt-1">
            {(!conclusion?.signatures || conclusion.signatures.length === 0) ? (
              <p className="text-xs text-gray-500 italic">Chưa có đại biểu nào ký xác nhận.</p>
            ) : (
              conclusion.signatures.map((sig, idx) => (
                <div
                  key={idx}
                  className="flex items-center justify-between bg-gray-800 p-2.5 rounded-lg border border-gray-700/80 text-xs"
                >
                  <div className="flex items-center gap-2">
                    <CheckCircle size={14} className="text-emerald-400" />
                    <span className="font-medium text-gray-200">{sig.fullName}</span>
                  </div>
                  <span className="text-gray-400">
                    {new Date(sig.signedAt).toLocaleTimeString('vi-VN')} {new Date(sig.signedAt).toLocaleDateString('vi-VN')}
                  </span>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
