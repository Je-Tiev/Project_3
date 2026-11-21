import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useApp } from "../contexts/AppContext";
import { useMeetingWithWebRTC } from "../hook/useMeeting";
import VideoTile from "../components/VideoTile";
import {
  Mic,
  MicOff,
  Video,
  VideoOff,
  Share2,
  PhoneOff,
  MessageSquare,
  Vote,
  Edit3,
  FileCheck,
  FileText,
  X,
  Users,
  Lock,
  Unlock,
  VolumeX,
  UserX,
  Disc,
} from "lucide-react";
import { apiCall } from "../utils/api";
import MeetingChatPanel from "../components/meeting/MeetingChatPanel";
import MeetingPollsPanel from "../components/meeting/MeetingPollsPanel";
import MeetingNotesPanel from "../components/meeting/MeetingNotesPanel";
import MeetingConclusionPanel from "../components/meeting/MeetingConclusionPanel";
import MeetingDocsPanel from "../components/meeting/MeetingDocsPanel";
import MeetingAudioRecorder from "../components/meeting/MeetingAudioRecorder";

export default function MeetingDetailPage() {
  const { meetingId } = useParams();
  const { currentUser, meetings } = useApp();
  const navigate = useNavigate();

  const [activePanel, setActivePanel] = useState(null); // 'chat' | 'polls' | 'notes' | 'conclusion' | 'docs' | 'recorder' | null
  const [meetingDetails, setMeetingDetails] = useState(null);
  const [showLeaveMessage, setShowLeaveMessage] = useState(false);
  const [syncedSlideInfo, setSyncedSlideInfo] = useState(null);
  const [hostNotification, setHostNotification] = useState("");

  // Load meeting metadata
  useEffect(() => {
    let isMounted = true;
    const loadMeeting = async () => {
      let found = meetings.find((m) => `${m.id}` === `${meetingId}` || `${m.meetingId}` === `${meetingId}`);
      if (!found) {
        try {
          const res = await apiCall(`/Meetings/${meetingId}`, { method: "GET" });
          if (res) found = res;
        } catch (e) {
          console.warn("Không thể tải thông tin cuộc họp:", e);
        }
      }
      if (isMounted && found) {
        setMeetingDetails(found);
      }
    };

    if (meetingId) {
      loadMeeting();
    }

    return () => {
      isMounted = false;
    };
  }, [meetingId, meetings]);

  const {
    status,
    participants,
    remoteStreams,
    messages,
    toggleCamera,
    toggleMicrophone,
    startScreenShare,
    stopScreenShare,
    sendMessage,
    connection,
    error,
    localStream,
    isMicOn,
    isVideoOn,
    isScreenSharing,
    isRoomLocked,
    muteAll,
    muteUser,
    kickParticipant,
    lockRoom,
    broadcastSlide,
  } = useMeetingWithWebRTC(meetingId);

  // Hook into Slide Sync event
  useEffect(() => {
    if (!connection) return;

    const handleSlideChanged = (data) => {
      setSyncedSlideInfo(data);
    };

    connection.on("SlidePageChanged", handleSlideChanged);

    return () => {
      connection.off("SlidePageChanged", handleSlideChanged);
    };
  }, [connection]);

  const handleLeaveMeeting = () => {
    setShowLeaveMessage(true);
    setTimeout(() => {
      navigate("/");
    }, 800);
  };

  const isHostOrAdmin =
    currentUser?.role === "Admin" ||
    currentUser?.role === "SuperAdmin" ||
    meetingDetails?.createdByUserId === currentUser?.userId ||
    meetingDetails?.createdBy === currentUser?.userId;

  const togglePanel = (panelName) => {
    setActivePanel((prev) => (prev === panelName ? null : panelName));
  };

  const handleMuteAll = async () => {
    if (!window.confirm("Bạn có muốn tắt micro của tất cả đại biểu tham gia?")) return;
    try {
      await muteAll();
      showToast("Đã gửi lệnh tắt micro tới tất cả đại biểu.");
    } catch (e) {
      alert("Lỗi: " + e.message);
    }
  };

  const handleToggleLock = async () => {
    try {
      await lockRoom(!isRoomLocked);
      showToast(isRoomLocked ? "Đã mở khóa phòng họp." : "Đã khóa phòng họp thành công.");
    } catch (e) {
      alert("Lỗi: " + e.message);
    }
  };

  const handleKick = async (targetConnId, name) => {
    if (!window.confirm(`Bạn có chắc muốn mời ${name || "đại biểu"} ra khỏi phòng họp?`)) return;
    try {
      await kickParticipant(targetConnId);
      showToast(`Đã mời ${name || "đại biểu"} rời khỏi phòng.`);
    } catch (e) {
      alert("Lỗi: " + e.message);
    }
  };

  const showToast = (msg) => {
    setHostNotification(msg);
    setTimeout(() => setHostNotification(""), 3500);
  };

  if (status === "connecting" || status === "reconnecting")
    return (
      <div className="flex h-screen items-center justify-center bg-gray-950 text-white flex-col gap-3">
        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-red-600" />
        <p className="text-gray-300">Đang kết nối vào phòng họp...</p>
      </div>
    );

  if (status === "error" || error || status === "kicked")
    return (
      <div className="flex h-screen items-center justify-center bg-gray-950 text-red-400 flex-col gap-4">
        <p className="text-lg font-medium">{error || "Lỗi khi tham gia cuộc họp"}</p>
        <button
          onClick={() => navigate("/")}
          className="px-4 py-2 bg-gray-800 hover:bg-gray-700 text-white rounded-lg text-sm"
        >
          Quay lại trang chủ
        </button>
      </div>
    );

  return (
    <div className="h-screen w-screen bg-gray-950 text-white flex flex-col overflow-hidden">
      {/* Top Header Bar */}
      <div className="h-14 bg-gray-900 border-b border-gray-800 px-4 flex items-center justify-between shrink-0">
        <div className="flex items-center gap-3">
          <div className="w-2.5 h-2.5 rounded-full bg-emerald-500 animate-pulse" />
          <h1 className="font-semibold text-sm sm:text-base text-gray-100 truncate max-w-xs sm:max-w-md">
            {meetingDetails?.title || `Phòng họp #${meetingId}`}
          </h1>
          <span className="hidden sm:inline-flex items-center gap-1 text-xs bg-gray-800 px-2.5 py-1 rounded-full text-gray-300">
            <Users size={12} /> {participants.length + 1} người
          </span>
          {isRoomLocked && (
            <span className="flex items-center gap-1 text-[11px] bg-red-950/80 border border-red-800 text-red-300 px-2 py-0.5 rounded-full">
              <Lock size={10} /> Đã khóa phòng
            </span>
          )}
        </div>

        {/* Quick Tab Buttons on Header */}
        <div className="flex items-center gap-1">
          {/* Host Controls */}
          {isHostOrAdmin && (
            <div className="flex items-center gap-1 mr-2 border-r border-gray-700 pr-2">
              <button
                onClick={handleMuteAll}
                className="p-2 rounded-lg text-xs flex items-center gap-1 text-yellow-400 hover:bg-yellow-950/40 transition-colors"
                title="Tắt micro tất cả đại biểu"
              >
                <VolumeX size={16} />
                <span className="hidden lg:inline">Tắt mic tất cả</span>
              </button>
              <button
                onClick={handleToggleLock}
                className={`p-2 rounded-lg text-xs flex items-center gap-1 transition-colors ${
                  isRoomLocked ? "text-red-400 bg-red-950/50" : "text-gray-300 hover:bg-gray-800"
                }`}
                title={isRoomLocked ? "Mở khóa phòng" : "Khóa phòng họp"}
              >
                {isRoomLocked ? <Lock size={16} /> : <Unlock size={16} />}
                <span className="hidden lg:inline">{isRoomLocked ? "Mở khóa" : "Khóa phòng"}</span>
              </button>
            </div>
          )}

          <button
            onClick={() => togglePanel("recorder")}
            className={`p-2 rounded-lg text-xs flex items-center gap-1.5 transition-colors ${
              activePanel === "recorder" ? "bg-red-600 text-white" : "text-gray-300 hover:bg-gray-800"
            }`}
            title="Ghi âm phiên họp"
          >
            <Disc size={16} />
            <span className="hidden md:inline">Ghi âm</span>
          </button>

          <button
            onClick={() => togglePanel("docs")}
            className={`p-2 rounded-lg text-xs flex items-center gap-1.5 transition-colors ${
              activePanel === "docs" ? "bg-red-600 text-white" : "text-gray-300 hover:bg-gray-800"
            }`}
            title="Tài liệu cuộc họp & Đồng bộ Slide"
          >
            <FileText size={16} />
            <span className="hidden md:inline">Tài liệu</span>
          </button>

          <button
            onClick={() => togglePanel("polls")}
            className={`p-2 rounded-lg text-xs flex items-center gap-1.5 transition-colors ${
              activePanel === "polls" ? "bg-red-600 text-white" : "text-gray-300 hover:bg-gray-800"
            }`}
            title="Biểu quyết số"
          >
            <Vote size={16} />
            <span className="hidden md:inline">Biểu quyết</span>
          </button>

          <button
            onClick={() => togglePanel("notes")}
            className={`p-2 rounded-lg text-xs flex items-center gap-1.5 transition-colors ${
              activePanel === "notes" ? "bg-red-600 text-white" : "text-gray-300 hover:bg-gray-800"
            }`}
            title="Ghi chú cá nhân"
          >
            <Edit3 size={16} />
            <span className="hidden md:inline">Ghi chú</span>
          </button>

          <button
            onClick={() => togglePanel("conclusion")}
            className={`p-2 rounded-lg text-xs flex items-center gap-1.5 transition-colors ${
              activePanel === "conclusion" ? "bg-red-600 text-white" : "text-gray-300 hover:bg-gray-800"
            }`}
            title="Biên bản & Kết luận"
          >
            <FileCheck size={16} />
            <span className="hidden md:inline">Kết luận</span>
          </button>

          <button
            onClick={() => togglePanel("chat")}
            className={`p-2 rounded-lg text-xs flex items-center gap-1.5 transition-colors relative ${
              activePanel === "chat" ? "bg-red-600 text-white" : "text-gray-300 hover:bg-gray-800"
            }`}
            title="Trò chuyện"
          >
            <MessageSquare size={16} />
            <span className="hidden md:inline">Chat</span>
            {messages.length > 0 && activePanel !== "chat" && (
              <span className="w-2 h-2 rounded-full bg-red-500 absolute top-1.5 right-1.5" />
            )}
          </button>
        </div>
      </div>

      {/* Host Toast Notification */}
      {hostNotification && (
        <div className="fixed top-16 left-1/2 transform -translate-x-1/2 bg-blue-600 text-white px-4 py-2 rounded-full text-xs shadow-xl z-50 flex items-center gap-2">
          <span>{hostNotification}</span>
        </div>
      )}

      {/* Main Workspace: Video Grid + Optional Side Panel */}
      <div className="flex-1 flex overflow-hidden relative">
        {/* VIDEO GRID */}
        <div className="flex-1 p-4 overflow-y-auto pb-24">
          <div
            className="grid gap-4 h-full"
            style={{
              gridTemplateColumns:
                participants.length === 0
                  ? "1fr"
                  : participants.length === 1
                  ? "repeat(auto-fit, minmax(320px, 1fr))"
                  : "repeat(auto-fit, minmax(280px, 1fr))",
            }}
          >
            <VideoTile
              connectionId="local"
              fullName={currentUser?.fullName || "Bạn"}
              stream={localStream}
              isLocal={true}
              isScreenSharing={isScreenSharing}
              camEnabled={isVideoOn}
              micEnabled={isMicOn}
              onToggleCam={toggleCamera}
              onToggleMic={toggleMicrophone}
            />

            {participants.map((p) => (
              <div key={p.connectionId} className="relative group">
                <VideoTile
                  connectionId={p.connectionId}
                  fullName={p.fullName}
                  stream={remoteStreams[p.connectionId]}
                  camEnabled={p.isVideoEnabled !== false}
                  micEnabled={p.isMicEnabled !== false}
                />
                {/* Host Quick Actions on Tile */}
                {isHostOrAdmin && (
                  <div className="absolute top-2 right-2 hidden group-hover:flex items-center gap-1 bg-black/60 backdrop-blur-xs p-1 rounded-lg z-10">
                    <button
                      onClick={() => muteUser(p.connectionId)}
                      className="p-1 text-gray-300 hover:text-yellow-400 hover:bg-gray-800 rounded"
                      title="Tắt micro đại biểu này"
                    >
                      <MicOff size={14} />
                    </button>
                    <button
                      onClick={() => handleKick(p.connectionId, p.fullName)}
                      className="p-1 text-gray-300 hover:text-red-400 hover:bg-gray-800 rounded"
                      title="Mời ra khỏi phòng"
                    >
                      <UserX size={14} />
                    </button>
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>

        {/* SIDE PANEL */}
        {activePanel && (
          <div className="w-full sm:w-96 md:w-[420px] h-full shrink-0 flex flex-col z-20 shadow-2xl relative">
            <button
              onClick={() => setActivePanel(null)}
              className="absolute top-3.5 right-3.5 p-1 rounded-lg text-gray-400 hover:text-white hover:bg-gray-700 transition-colors z-30"
              title="Đóng bảng"
            >
              <X size={18} />
            </button>

            {activePanel === "recorder" && (
              <div className="flex flex-col h-full bg-gray-800 text-white border-l border-gray-700 p-4 space-y-4">
                <h3 className="font-semibold text-base flex items-center gap-2">
                  <Disc className="text-red-500" /> Bản ghi âm cuộc họp
                </h3>
                <MeetingAudioRecorder
                  meetingId={meetingId}
                  onRecordingSaved={() => showToast("Đã lưu bản ghi âm thành công!")}
                />
              </div>
            )}

            {activePanel === "chat" && (
              <MeetingChatPanel
                messages={messages}
                onSendMessage={sendMessage}
                currentUserId={currentUser?.userId}
              />
            )}

            {activePanel === "polls" && (
              <MeetingPollsPanel
                meetingId={meetingId}
                connection={connection}
                isHostOrAdmin={isHostOrAdmin}
                currentUserId={currentUser?.userId}
              />
            )}

            {activePanel === "notes" && <MeetingNotesPanel meetingId={meetingId} />}

            {activePanel === "conclusion" && (
              <MeetingConclusionPanel
                meetingId={meetingId}
                connection={connection}
                isHostOrAdmin={isHostOrAdmin}
                currentUserId={currentUser?.userId}
                meetingTitle={meetingDetails?.title}
              />
            )}

            {activePanel === "docs" && (
              <MeetingDocsPanel
                meetingId={meetingId}
                isPresenter={isHostOrAdmin}
                onBroadcastSlideChange={({ documentId, pageNumber, zoomLevel }) =>
                  broadcastSlide(documentId, pageNumber, zoomLevel)
                }
                syncedSlideInfo={syncedSlideInfo}
              />
            )}
          </div>
        )}
      </div>

      {/* BOTTOM CONTROL BAR */}
      <div className="fixed bottom-4 left-1/2 transform -translate-x-1/2 bg-gray-900/90 backdrop-blur-md px-5 py-2.5 rounded-full border border-gray-700/80 shadow-2xl flex items-center gap-3 z-30">
        {/* MIC */}
        <button
          onClick={toggleMicrophone}
          className={`p-3 rounded-full transition-all ${
            isMicOn ? "bg-gray-800 hover:bg-gray-700 text-white" : "bg-red-600 hover:bg-red-500 text-white"
          }`}
          title={isMicOn ? "Tắt Micro" : "Bật Micro"}
        >
          {isMicOn ? <Mic size={20} /> : <MicOff size={20} />}
        </button>

        {/* CAMERA */}
        <button
          onClick={toggleCamera}
          className={`p-3 rounded-full transition-all ${
            isVideoOn ? "bg-gray-800 hover:bg-gray-700 text-white" : "bg-red-600 hover:bg-red-500 text-white"
          }`}
          title={isVideoOn ? "Tắt Camera" : "Bật Camera"}
        >
          {isVideoOn ? <Video size={20} /> : <VideoOff size={20} />}
        </button>

        {/* SHARE SCREEN */}
        <button
          onClick={isScreenSharing ? stopScreenShare : startScreenShare}
          className={`p-3 rounded-full transition-all ${
            isScreenSharing
              ? "bg-blue-600 hover:bg-blue-500 text-white ring-2 ring-blue-400/50"
              : "bg-gray-800 hover:bg-gray-700 text-gray-200"
          }`}
          title={isScreenSharing ? "Dừng chia sẻ màn hình" : "Chia sẻ màn hình"}
        >
          <Share2 size={20} />
        </button>

        <div className="w-px h-6 bg-gray-700 mx-1" />

        {/* LEAVE MEETING */}
        <button
          onClick={handleLeaveMeeting}
          className="px-4 py-2.5 rounded-full bg-red-600 hover:bg-red-700 text-white font-medium text-xs sm:text-sm flex items-center gap-2 shadow-lg shadow-red-600/30 transition-all"
          title="Rời cuộc họp"
        >
          <PhoneOff size={18} />
          <span>Rời phòng</span>
        </button>
      </div>

      {/* Leave Feedback Toast */}
      {showLeaveMessage && (
        <div className="fixed inset-0 bg-black/70 backdrop-blur-xs flex items-center justify-center z-50">
          <div className="bg-gray-900 border border-gray-700 p-6 rounded-2xl text-center space-y-2">
            <h3 className="text-base font-semibold text-white">Đang rời khỏi phiên họp...</h3>
            <p className="text-xs text-gray-400">Cảm ơn bạn đã tham gia.</p>
          </div>
        </div>
      )}
    </div>
  );
}
