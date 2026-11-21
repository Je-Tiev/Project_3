import React, { useState, useEffect } from 'react';
import { Vote, Plus, CheckCircle, XCircle, Lock, Shield, BarChart3, AlertCircle } from 'lucide-react';
import { apiCall } from '../../utils/api';

export default function MeetingPollsPanel({ meetingId, connection, isHostOrAdmin, currentUserId }) {
  const [polls, setPolls] = useState([]);
  const [loading, setLoading] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newQuestion, setNewQuestion] = useState('');
  const [isSecret, setIsSecret] = useState(false);
  const [votingId, setVotingId] = useState(null);
  const [errorMsg, setErrorMsg] = useState('');

  // Fetch polls on load
  const loadPolls = async () => {
    try {
      setLoading(true);
      const res = await apiCall(`/Polls/meeting/${meetingId}`, { method: 'GET' });
      if (Array.isArray(res)) {
        setPolls(res);
      }
    } catch (err) {
      console.error('Lỗi tải danh sách biểu quyết:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (meetingId) {
      loadPolls();
    }
  }, [meetingId]);

  // Hook into SignalR events
  useEffect(() => {
    if (!connection) return;

    const handleNewPoll = (poll) => {
      setPolls((prev) => [poll, ...prev.filter((p) => p.pollId !== poll.pollId)]);
    };

    const handleUpdateStats = (updatedPoll) => {
      setPolls((prev) =>
        prev.map((p) => (p.pollId === updatedPoll.pollId ? updatedPoll : p))
      );
    };

    const handlePollClosed = ({ pollId }) => {
      setPolls((prev) =>
        prev.map((p) => (p.pollId === pollId ? { ...p, status: 'Closed' } : p))
      );
    };

    connection.on('ReceiveNewPoll', handleNewPoll);
    connection.on('UpdatePollStats', handleUpdateStats);
    connection.on('PollClosed', handlePollClosed);

    return () => {
      connection.off('ReceiveNewPoll', handleNewPoll);
      connection.off('UpdatePollStats', handleUpdateStats);
      connection.off('PollClosed', handlePollClosed);
    };
  }, [connection]);

  // Handle Create Poll
  const handleCreatePoll = async (e) => {
    e.preventDefault();
    if (!newQuestion.trim()) return;

    try {
      await apiCall('/Polls', {
        method: 'POST',
        body: JSON.stringify({
          meetingId: Number(meetingId),
          question: newQuestion.trim(),
          isSecret,
        }),
      });
      setNewQuestion('');
      setIsSecret(false);
      setShowCreateModal(false);
      loadPolls();
    } catch (err) {
      console.error('Lỗi tạo biểu quyết:', err);
      alert('Không thể tạo biểu quyết: ' + (err.message || 'Lỗi hệ thống'));
    }
  };

  // Handle Vote
  const handleVote = async (pollId, choice) => {
    try {
      setVotingId(pollId);
      setErrorMsg('');
      await apiCall(`/Polls/${pollId}/vote`, {
        method: 'POST',
        body: JSON.stringify({ choice }),
      });
      // Refresh local stats
      const updated = await apiCall(`/Polls/${pollId}`, { method: 'GET' });
      setPolls((prev) => prev.map((p) => (p.pollId === pollId ? updated : p)));
    } catch (err) {
      setErrorMsg(err.message || 'Lỗi khi bỏ phiếu');
    } finally {
      setVotingId(null);
    }
  };

  // Handle Close Poll
  const handleClosePoll = async (pollId) => {
    if (!window.confirm('Bạn có chắc chắn muốn kết thúc cuộc biểu quyết này?')) return;
    try {
      await apiCall(`/Polls/${pollId}/close`, { method: 'PUT' });
      setPolls((prev) =>
        prev.map((p) => (p.pollId === pollId ? { ...p, status: 'Closed' } : p))
      );
    } catch (err) {
      alert('Lỗi đóng biểu quyết: ' + err.message);
    }
  };

  return (
    <div className="flex flex-col h-full bg-gray-800 text-white border-l border-gray-700">
      {/* Header */}
      <div className="p-4 border-b border-gray-700 flex items-center justify-between">
        <div className="flex items-center gap-2 font-semibold">
          <Vote size={18} className="text-red-500" />
          <span>Biểu quyết ({polls.length})</span>
        </div>
        {isHostOrAdmin && (
          <button
            onClick={() => setShowCreateModal(true)}
            className="flex items-center gap-1 text-xs bg-red-600 hover:bg-red-700 px-3 py-1.5 rounded-lg transition-colors font-medium"
          >
            <Plus size={14} /> Tạo mới
          </button>
        )}
      </div>

      {errorMsg && (
        <div className="m-3 p-2 bg-red-900/60 border border-red-700 text-red-200 text-xs rounded-lg flex items-center gap-2">
          <AlertCircle size={14} /> {errorMsg}
        </div>
      )}

      {/* Polls List */}
      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {loading && polls.length === 0 ? (
          <div className="text-center text-gray-400 py-8 text-sm">Đang tải biểu quyết...</div>
        ) : polls.length === 0 ? (
          <div className="text-center text-gray-400 py-8 text-sm">
            Chưa có câu hỏi biểu quyết nào trong phiên họp này.
          </div>
        ) : (
          polls.map((poll) => {
            const isOpen = poll.status === 'Open';
            const total = poll.totalVotes || 0;
            const yesPercent = total > 0 ? Math.round(((poll.yesVotes || 0) / total) * 100) : 0;
            const noPercent = total > 0 ? Math.round(((poll.noVotes || 0) / total) * 100) : 0;

            return (
              <div
                key={poll.pollId}
                className="bg-gray-750 p-4 rounded-xl border border-gray-700 space-y-3"
              >
                {/* Status Badges */}
                <div className="flex items-center justify-between text-xs">
                  <div className="flex items-center gap-2">
                    <span
                      className={`px-2 py-0.5 rounded-full font-medium ${
                        isOpen ? 'bg-green-900/80 text-green-300' : 'bg-gray-700 text-gray-400'
                      }`}
                    >
                      {isOpen ? 'Đang mở' : 'Đã kết thúc'}
                    </span>
                    {poll.isSecret && (
                      <span className="flex items-center gap-1 bg-purple-900/80 text-purple-300 px-2 py-0.5 rounded-full">
                        <Lock size={10} /> Bỏ phiếu kín
                      </span>
                    )}
                  </div>
                  <span className="text-gray-400">Tổng: {total} phiếu</span>
                </div>

                {/* Question */}
                <h4 className="font-medium text-sm text-gray-100">{poll.question}</h4>

                {/* Vote Buttons (if Open & User hasn't voted yet) */}
                {isOpen && !poll.userHasVoted && (
                  <div className="grid grid-cols-2 gap-2 pt-1">
                    <button
                      onClick={() => handleVote(poll.pollId, true)}
                      disabled={votingId === poll.pollId}
                      className="flex items-center justify-center gap-1.5 py-2 px-3 bg-emerald-600 hover:bg-emerald-500 rounded-lg text-xs font-medium transition-colors"
                    >
                      <CheckCircle size={14} /> Đồng ý
                    </button>
                    <button
                      onClick={() => handleVote(poll.pollId, false)}
                      disabled={votingId === poll.pollId}
                      className="flex items-center justify-center gap-1.5 py-2 px-3 bg-rose-600 hover:bg-rose-500 rounded-lg text-xs font-medium transition-colors"
                    >
                      <XCircle size={14} /> Không đồng ý
                    </button>
                  </div>
                )}

                {poll.userHasVoted && (
                  <div className="text-xs text-gray-300 bg-gray-800/80 p-2 rounded-lg flex items-center justify-between border border-gray-700">
                    <span>Bạn đã bỏ phiếu:</span>
                    <span className={`font-semibold ${poll.userChoice ? 'text-emerald-400' : 'text-rose-400'}`}>
                      {poll.userChoice ? 'Đồng ý' : 'Không đồng ý'}
                    </span>
                  </div>
                )}

                {/* Result Progress Bars */}
                <div className="space-y-2 pt-1 text-xs">
                  <div>
                    <div className="flex justify-between text-gray-300 mb-1">
                      <span>Đồng ý ({poll.yesVotes || 0})</span>
                      <span>{yesPercent}%</span>
                    </div>
                    <div className="w-full bg-gray-700 rounded-full h-2 overflow-hidden">
                      <div
                        className="bg-emerald-500 h-2 rounded-full transition-all duration-500"
                        style={{ width: `${yesPercent}%` }}
                      />
                    </div>
                  </div>

                  <div>
                    <div className="flex justify-between text-gray-300 mb-1">
                      <span>Không đồng ý ({poll.noVotes || 0})</span>
                      <span>{noPercent}%</span>
                    </div>
                    <div className="w-full bg-gray-700 rounded-full h-2 overflow-hidden">
                      <div
                        className="bg-rose-500 h-2 rounded-full transition-all duration-500"
                        style={{ width: `${noPercent}%` }}
                      />
                    </div>
                  </div>
                </div>

                {/* Host Close Poll button */}
                {isHostOrAdmin && isOpen && (
                  <button
                    onClick={() => handleClosePoll(poll.pollId)}
                    className="w-full mt-2 py-1.5 text-xs text-gray-400 hover:text-red-400 hover:bg-gray-700/50 rounded transition-colors"
                  >
                    Đóng biểu quyết
                  </button>
                )}
              </div>
            );
          })
        )}
      </div>

      {/* Modal Tạo Biểu Quyết */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-xs flex items-center justify-center z-50 p-4">
          <div className="bg-gray-800 border border-gray-700 rounded-2xl w-full max-w-md p-6 shadow-2xl">
            <h3 className="text-lg font-bold mb-4 flex items-center gap-2">
              <Vote className="text-red-500" /> Tạo câu hỏi biểu quyết
            </h3>
            <form onSubmit={handleCreatePoll} className="space-y-4">
              <div>
                <label className="block text-sm text-gray-300 mb-1">Nội dung câu hỏi</label>
                <textarea
                  rows={3}
                  value={newQuestion}
                  onChange={(e) => setNewQuestion(e.target.value)}
                  placeholder="Nhập nội dung vấn đề cần biểu quyết..."
                  className="w-full bg-gray-700 border border-gray-600 rounded-lg p-3 text-sm text-white focus:outline-none focus:border-red-500"
                  required
                />
              </div>

              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="secretCheckbox"
                  checked={isSecret}
                  onChange={(e) => setIsSecret(e.target.checked)}
                  className="w-4 h-4 rounded text-red-600 focus:ring-red-500 bg-gray-700 border-gray-600"
                />
                <label htmlFor="secretCheckbox" className="text-sm text-gray-200 cursor-pointer">
                  Bỏ phiếu kín (Ẩn danh tính đại biểu bỏ phiếu)
                </label>
              </div>

              <div className="flex justify-end gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="px-4 py-2 bg-gray-700 hover:bg-gray-600 rounded-lg text-sm text-gray-300"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="px-5 py-2 bg-red-600 hover:bg-red-700 rounded-lg text-sm font-medium text-white shadow-lg shadow-red-600/30"
                >
                  Bắt đầu biểu quyết
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
