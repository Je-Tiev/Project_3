import React, { useState, useRef, useEffect } from 'react';
import { Mic, Square, Play, Upload, CheckCircle2, AlertCircle, Disc } from 'lucide-react';

export default function MeetingAudioRecorder({ meetingId, onRecordingSaved }) {
  const [isRecording, setIsRecording] = useState(false);
  const [isPaused, setIsPaused] = useState(false);
  const [recordSeconds, setRecordSeconds] = useState(0);
  const [audioBlob, setAudioBlob] = useState(null);
  const [audioUrl, setAudioUrl] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [uploadSuccess, setUploadSuccess] = useState(false);
  const [errorMsg, setErrorMsg] = useState('');

  const mediaRecorderRef = useRef(null);
  const audioChunksRef = useRef([]);
  const timerRef = useRef(null);
  const streamRef = useRef(null);

  useEffect(() => {
    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
      if (streamRef.current) {
        streamRef.current.getTracks().forEach((t) => t.stop());
      }
      if (audioUrl) {
        window.URL.revokeObjectURL(audioUrl);
      }
    };
  }, [audioUrl]);

  // Start Recording
  const startRecording = async () => {
    setErrorMsg('');
    setUploadSuccess(false);
    setAudioBlob(null);
    if (audioUrl) window.URL.revokeObjectURL(audioUrl);
    setAudioUrl(null);
    audioChunksRef.current = [];

    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        audio: {
          echoCancellation: true,
          noiseSuppression: true,
          sampleRate: 44100,
        },
      });
      streamRef.current = stream;

      const mimeType = MediaRecorder.isTypeSupported('audio/webm;codecs=opus')
        ? 'audio/webm;codecs=opus'
        : 'audio/webm';

      const mediaRecorder = new MediaRecorder(stream, { mimeType });
      mediaRecorderRef.current = mediaRecorder;

      mediaRecorder.ondataavailable = (event) => {
        if (event.data && event.data.size > 0) {
          audioChunksRef.current.push(event.data);
        }
      };

      mediaRecorder.onstop = () => {
        const blob = new Blob(audioChunksRef.current, { type: 'audio/webm' });
        setAudioBlob(blob);
        const url = window.URL.createObjectURL(blob);
        setAudioUrl(url);

        if (streamRef.current) {
          streamRef.current.getTracks().forEach((t) => t.stop());
          streamRef.current = null;
        }
      };

      mediaRecorder.start(1000); // chunk every 1s
      setIsRecording(true);
      setRecordSeconds(0);

      timerRef.current = setInterval(() => {
        setRecordSeconds((prev) => prev + 1);
      }, 1000);
    } catch (err) {
      console.error('Không thể truy cập micro:', err);
      setErrorMsg('Không thể truy cập Microphone để ghi âm cuộc họp.');
    }
  };

  // Stop Recording
  const stopRecording = () => {
    if (mediaRecorderRef.current && mediaRecorderRef.current.state !== 'inactive') {
      mediaRecorderRef.current.stop();
    }
    if (timerRef.current) clearInterval(timerRef.current);
    setIsRecording(false);
  };

  // Upload to Backend
  const handleUpload = async () => {
    if (!audioBlob) return;

    setUploading(true);
    setErrorMsg('');
    try {
      const formData = new FormData();
      formData.append('file', audioBlob, `recording_meeting_${meetingId}_${Date.now()}.webm`);
      formData.append('duration', recordSeconds);

      const token = localStorage.getItem('token');
      const baseUrl = import.meta.env.VITE_API_URL || '/api';

      const response = await fetch(`${baseUrl}/Audios/meeting/${meetingId}/upload`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
        },
        body: formData,
      });

      if (!response.ok) {
        const errJson = await response.json().catch(() => null);
        throw new Error(errJson?.message || `HTTP ${response.status}`);
      }

      const result = await response.json();
      setUploadSuccess(true);
      if (onRecordingSaved) {
        onRecordingSaved(result);
      }
    } catch (err) {
      console.error('Lỗi tải file ghi âm:', err);
      setErrorMsg('Lỗi khi lưu bản ghi âm: ' + err.message);
    } finally {
      setUploading(false);
    }
  };

  const formatTime = (secs) => {
    const mins = Math.floor(secs / 60);
    const rem = secs % 60;
    return `${mins.toString().padStart(2, '0')}:${rem.toString().padStart(2, '0')}`;
  };

  return (
    <div className="bg-gray-800 p-4 rounded-xl border border-gray-700 text-white space-y-3">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Disc className={`w-5 h-5 ${isRecording ? 'text-red-500 animate-spin' : 'text-gray-400'}`} />
          <span className="font-semibold text-sm">Ghi âm phiên họp</span>
        </div>
        {isRecording && (
          <div className="flex items-center gap-1.5 text-xs text-red-400 font-mono">
            <span className="w-2 h-2 rounded-full bg-red-500 animate-ping" />
            <span>{formatTime(recordSeconds)}</span>
          </div>
        )}
      </div>

      {errorMsg && (
        <div className="p-2.5 bg-red-900/60 border border-red-700 rounded-lg text-xs text-red-200 flex items-center gap-2">
          <AlertCircle size={14} className="shrink-0" />
          <span>{errorMsg}</span>
        </div>
      )}

      {uploadSuccess && (
        <div className="p-2.5 bg-emerald-900/60 border border-emerald-700 rounded-lg text-xs text-emerald-200 flex items-center gap-2">
          <CheckCircle2 size={14} className="shrink-0" />
          <span>Đã lưu bản ghi âm lên máy chủ thành công!</span>
        </div>
      )}

      {/* Control Buttons */}
      <div className="flex items-center gap-2 pt-1">
        {!isRecording ? (
          <button
            onClick={startRecording}
            disabled={uploading}
            className="flex items-center gap-1.5 px-3.5 py-2 bg-red-600 hover:bg-red-700 disabled:opacity-50 text-white rounded-lg text-xs font-medium transition-colors shadow-sm"
          >
            <Mic size={14} /> Bắt đầu ghi âm
          </button>
        ) : (
          <button
            onClick={stopRecording}
            className="flex items-center gap-1.5 px-3.5 py-2 bg-gray-700 hover:bg-gray-600 text-white rounded-lg text-xs font-medium transition-colors"
          >
            <Square size={14} /> Dừng ghi âm ({formatTime(recordSeconds)})
          </button>
        )}

        {audioUrl && !isRecording && (
          <>
            <audio src={audioUrl} controls className="h-8 max-w-[180px] rounded" />
            <button
              onClick={handleUpload}
              disabled={uploading || uploadSuccess}
              className="flex items-center gap-1.5 px-3 py-2 bg-emerald-600 hover:bg-emerald-500 disabled:opacity-50 text-white rounded-lg text-xs font-medium transition-colors"
            >
              <Upload size={14} /> {uploading ? 'Đang lưu...' : 'Lưu lên Server'}
            </button>
          </>
        )}
      </div>
    </div>
  );
}
