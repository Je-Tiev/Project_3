import React, { useState } from "react";
import { Save, X, Plus, Trash2, Upload } from "lucide-react";
import { useApp } from "../../contexts/AppContext";
import { dayOfWeekViFromDmy } from "../../utils/dateUtils";

const CreateMeetingForm = ({ onClose, onSave }) => {
  const { createMeeting } = useApp();
  const [activeSection, setActiveSection] = useState("basic");
  const [form, setForm] = useState({
    title: "",
    description: "",
    host: "",
    room: "",
    date: "",
    time: "",
    type: "internal",
    participants: [],
    documents: [],
    votingTopics: [],
    notifications: true,
    accessControl: false,
  });

  const [message, setMessage] = useState("");

  const sections = [
    { key: "basic", label: "Basic Info" },
    { key: "participants", label: "Participants" },
    { key: "documents", label: "Documents" },
    { key: "voting", label: "Voting Setup" },
    { key: "advanced", label: "Advanced Options" },
  ];

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm((prev) => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const handleAddParticipant = () => {
    const name = prompt("Enter participant name:");
    if (name) {
      setForm((prev) => ({
        ...prev,
        participants: [
          ...prev.participants,
          { id: Date.now(), name, role: "Participant" },
        ],
      }));
    }
  };

  const handleRemoveParticipant = (id) => {
    setForm((prev) => ({
      ...prev,
      participants: prev.participants.filter((p) => p.id !== id),
    }));
  };

  const handleAddVotingTopic = () => {
    const topic = prompt("Enter voting topic:");
    if (topic) {
      setForm((prev) => ({
        ...prev,
        votingTopics: [...prev.votingTopics, { id: Date.now(), topic }],
      }));
    }
  };

  const handleRemoveVotingTopic = (id) => {
    setForm((prev) => ({
      ...prev,
      votingTopics: prev.votingTopics.filter((v) => v.id !== id),
    }));
  };

  const handleFileUpload = (e, type) => {
    const file = e.target.files?.[0];
    if (file) {
      const url = URL.createObjectURL(file);
      setForm((prev) => ({
        ...prev,
        documents: [
          ...prev.documents,
          {
            id: Date.now(),
            name: file.name,
            type,
            url,
            file,
          },
        ],
      }));
    }
  };

  const handleRemoveDocument = (id) => {
    setForm((prev) => ({
      ...prev,
      documents: prev.documents.filter((d) => d.id !== id),
    }));
  };

  const handleSave = async (asDraft = false) => {
    if (!form.title || !form.date || !form.time) {
      setMessage("Please fill in required fields (Title, Date, Time)");
      setTimeout(() => setMessage(""), 3000);
      return;
    }

    try {
      const result = await createMeeting({
        title: form.title,
        description: form.description,
        date: form.date,
        time: form.time,
        location: form.room,
        organizer: form.host,
        session:
          form.time.includes("AM") || parseInt(form.time.split(":")[0]) < 12
            ? "Buổi sáng"
            : "Buổi chiều",
        status: asDraft ? "not_started" : "not_started",
        approved: !asDraft,
        participants: form.participants,
        documents: form.documents,
      });

      if (result.success) {
        setMessage(
          asDraft
            ? "✅ Saved as draft successfully"
            : "✅ Meeting created successfully"
        );
        setTimeout(() => {
          if (onSave) onSave();
          if (onClose) onClose();
        }, 1500);
      } else {
        setMessage(" Error: " + (result.error || "Failed to create meeting"));
      }
    } catch (error) {
      setMessage(" Error: " + error.message);
    }
  };

  return (
    <div className="p-6">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-text">Create New Meeting</h1>
        <div className="flex items-center gap-3">
          <button
            onClick={() => handleSave(false)}
            className="bg-primary hover:bg-primary-dark text-white px-6 py-2 rounded-button font-semibold flex items-center gap-2 transition-colors"
          >
            <Save className="w-5 h-5" />
            Save
          </button>
          <button
            onClick={() => handleSave(true)}
            className="bg-secondary hover:bg-secondary-dark text-text px-6 py-2 rounded-button font-semibold border border-secondary-dark transition-colors"
          >
            Save as Draft
          </button>
          <button
            onClick={onClose}
            className="bg-white hover:bg-secondary text-text px-6 py-2 rounded-button font-semibold border border-secondary-dark transition-colors flex items-center gap-2"
          >
            <X className="w-5 h-5" />
            Cancel
          </button>
        </div>
      </div>

      {message && (
        <div
          className={`mb-4 p-4 rounded-button ${
            message.includes("successfully")
              ? "bg-green-50 text-green-700 border border-green-200"
              : "bg-red-50 text-red-700 border border-red-200"
          }`}
        >
          {message}
        </div>
      )}

      {/* Sections Navigation */}
      <div className="bg-white border border-secondary-dark rounded-lg mb-6">
        <div className="flex border-b border-secondary-dark overflow-x-auto">
          {sections.map((section) => (
            <button
              key={section.key}
              onClick={() => setActiveSection(section.key)}
              className={`px-6 py-4 font-semibold text-sm transition-colors whitespace-nowrap ${
                activeSection === section.key
                  ? "text-primary border-b-2 border-primary"
                  : "text-text-light hover:text-text"
              }`}
            >
              {section.label}
            </button>
          ))}
        </div>

        {/* Section Content */}
        <div className="p-6">
          {activeSection === "basic" && (
            <div className="space-y-6 max-w-3xl">
              <div>
                <label className="block text-sm font-medium text-text mb-2">
                  Title <span className="text-primary">*</span>
                </label>
                <input
                  type="text"
                  name="title"
                  value={form.title}
                  onChange={handleChange}
                  className="w-full px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
                  placeholder="Enter meeting title"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text mb-2">
                  Description
                </label>
                <textarea
                  name="description"
                  value={form.description}
                  onChange={handleChange}
                  rows={4}
                  className="w-full px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
                  placeholder="Enter meeting description"
                />
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-text mb-2">
                    Host <span className="text-primary">*</span>
                  </label>
                  <input
                    type="text"
                    name="host"
                    value={form.host}
                    onChange={handleChange}
                    className="w-full px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
                    placeholder="Enter host name"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-text mb-2">
                    Room
                  </label>
                  <input
                    type="text"
                    name="room"
                    value={form.room}
                    onChange={handleChange}
                    className="w-full px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
                    placeholder="Enter room name"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-text mb-2">
                    Date <span className="text-primary">*</span>
                  </label>
                  <input
                    type="text"
                    name="date"
                    value={form.date}
                    onChange={handleChange}
                    className="w-full px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
                    placeholder="dd/mm/yyyy"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-text mb-2">
                    Time <span className="text-primary">*</span>
                  </label>
                  <input
                    type="text"
                    name="time"
                    value={form.time}
                    onChange={handleChange}
                    className="w-full px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
                    placeholder="HH:mm"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-text mb-2">
                    Type
                  </label>
                  <select
                    name="type"
                    value={form.type}
                    onChange={handleChange}
                    className="w-full px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
                  >
                    <option value="internal">Internal</option>
                    <option value="external">External</option>
                    <option value="public">Public</option>
                  </select>
                </div>
              </div>
            </div>
          )}

          {activeSection === "participants" && (
            <div className="space-y-4">
              <div className="flex justify-between items-center">
                <h3 className="text-lg font-semibold text-text">
                  Participants
                </h3>
                <button
                  onClick={handleAddParticipant}
                  className="bg-primary hover:bg-primary-dark text-white px-4 py-2 rounded-button font-semibold flex items-center gap-2 transition-colors"
                >
                  <Plus className="w-4 h-4" />
                  Add Participant
                </button>
              </div>
              {form.participants.length > 0 ? (
                <div className="bg-white border border-secondary-dark rounded-lg overflow-hidden">
                  <table className="w-full">
                    <thead className="bg-secondary">
                      <tr>
                        <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">
                          Name
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">
                          Role
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">
                          Actions
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-secondary-dark">
                      {form.participants.map((p, index) => (
                        <tr
                          key={p.id}
                          className={
                            index % 2 === 0 ? "bg-white" : "bg-secondary"
                          }
                        >
                          <td className="px-6 py-4 text-sm text-text font-medium">
                            {p.name}
                          </td>
                          <td className="px-6 py-4 text-sm text-text">
                            {p.role}
                          </td>
                          <td className="px-6 py-4">
                            <button
                              onClick={() => handleRemoveParticipant(p.id)}
                              className="text-red-600 hover:text-red-700 transition-colors"
                            >
                              <Trash2 className="w-4 h-4" />
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <p className="text-text-light text-center py-8">
                  No participants added yet.
                </p>
              )}
            </div>
          )}

          {activeSection === "documents" && (
            <div className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-text mb-2">
                    Tài liệu được phát
                  </label>
                  <div className="relative">
                    <input
                      type="text"
                      placeholder="Dán link hoặc bấm + để chọn file"
                      className="w-full pr-12 px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
                    />
                    <input
                      type="file"
                      onChange={(e) => handleFileUpload(e, "rev")}
                      className="hidden"
                      id="revUpload"
                    />
                    <button
                      type="button"
                      onClick={() =>
                        document.getElementById("revUpload").click()
                      }
                      className="absolute right-2 top-1/2 -translate-y-1/2 w-8 h-8 rounded-full bg-transparent hover:bg-transparent text-text text-lg leading-none"
                      title="Chọn file"
                    >
                      +
                    </button>
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-text mb-2">
                    Tài liệu chuẩn bị
                  </label>
                  <div className="relative">
                    <input
                      type="text"
                      placeholder="Dán link hoặc bấm + để chọn file"
                      className="w-full pr-12 px-4 py-2 border border-secondary-dark rounded-button text-text focus:outline-none focus:ring-2 focus:ring-primary"
                    />
                    <input
                      type="file"
                      onChange={(e) => handleFileUpload(e, "pre")}
                      className="hidden"
                      id="preUpload"
                    />
                    <button
                      type="button"
                      onClick={() =>
                        document.getElementById("preUpload").click()
                      }
                      className="absolute right-2 top-1/2 -translate-y-1/2 w-8 h-8 rounded-full bg-transparent hover:bg-transparent text-text text-lg leading-none"
                      title="Chọn file"
                    >
                      +
                    </button>
                  </div>
                </div>
              </div>
              {form.documents.length > 0 && (
                <div className="bg-white border border-secondary-dark rounded-lg overflow-hidden">
                  <table className="w-full">
                    <thead className="bg-secondary">
                      <tr>
                        <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">
                          File Name
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">
                          Type
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-semibold text-text-light uppercase">
                          Actions
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-secondary-dark">
                      {form.documents.map((doc, index) => (
                        <tr
                          key={doc.id}
                          className={
                            index % 2 === 0 ? "bg-white" : "bg-secondary"
                          }
                        >
                          <td className="px-6 py-4 text-sm text-text font-medium">
                            {doc.name}
                          </td>
                          <td className="px-6 py-4 text-sm text-text">
                            {doc.type === "rev"
                              ? "Tài liệu được phát"
                              : "Tài liệu chuẩn bị"}
                          </td>
                          <td className="px-6 py-4">
                            <button
                              onClick={() => handleRemoveDocument(doc.id)}
                              className="text-red-600 hover:text-red-700 transition-colors"
                            >
                              <Trash2 className="w-4 h-4" />
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          )}

          {activeSection === "voting" && (
            <div className="space-y-4">
              <div className="flex justify-between items-center">
                <h3 className="text-lg font-semibold text-text">
                  Voting Topics
                </h3>
                <button
                  onClick={handleAddVotingTopic}
                  className="bg-primary hover:bg-primary-dark text-white px-4 py-2 rounded-button font-semibold flex items-center gap-2 transition-colors"
                >
                  <Plus className="w-4 h-4" />
                  Add Topic
                </button>
              </div>
              {form.votingTopics.length > 0 ? (
                <div className="space-y-3">
                  {form.votingTopics.map((topic) => (
                    <div
                      key={topic.id}
                      className="bg-secondary border border-secondary-dark rounded-lg p-4 flex items-center justify-between"
                    >
                      <span className="text-text font-medium">
                        {topic.topic}
                      </span>
                      <button
                        onClick={() => handleRemoveVotingTopic(topic.id)}
                        className="text-red-600 hover:text-red-700 transition-colors"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-text-light text-center py-8">
                  No voting topics added yet.
                </p>
              )}
            </div>
          )}

          {activeSection === "advanced" && (
            <div className="space-y-6 max-w-2xl">
              <div className="flex items-center justify-between p-4 bg-secondary border border-secondary-dark rounded-lg">
                <div>
                  <label className="text-sm font-medium text-text">
                    Send Notifications
                  </label>
                  <p className="text-xs text-text-light">
                    Notify participants about the meeting
                  </p>
                </div>
                <input
                  type="checkbox"
                  name="notifications"
                  checked={form.notifications}
                  onChange={handleChange}
                  className="w-5 h-5 text-primary rounded focus:ring-primary"
                />
              </div>
              <div className="flex items-center justify-between p-4 bg-secondary border border-secondary-dark rounded-lg">
                <div>
                  <label className="text-sm font-medium text-text">
                    Access Control
                  </label>
                  <p className="text-xs text-text-light">
                    Restrict access to authorized users only
                  </p>
                </div>
                <input
                  type="checkbox"
                  name="accessControl"
                  checked={form.accessControl}
                  onChange={handleChange}
                  className="w-5 h-5 text-primary rounded focus:ring-primary"
                />
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Footer Bar */}
      <div className="bg-white border-t border-secondary-dark sticky bottom-0 left-0 right-0 p-4 flex justify-end gap-3 shadow-lg mt-6">
        <button
          onClick={() => handleSave(false)}
          className="bg-primary hover:bg-primary-dark text-white px-6 py-2 rounded-button font-semibold flex items-center gap-2 transition-colors"
        >
          <Save className="w-5 h-5" />
          Save & Create Meeting
        </button>
        <button
          onClick={() => handleSave(true)}
          className="bg-secondary hover:bg-secondary-dark text-text px-6 py-2 rounded-button font-semibold border border-secondary-dark transition-colors"
        >
          Save as Draft
        </button>
        <button
          onClick={onClose}
          className="bg-white hover:bg-secondary text-text px-6 py-2 rounded-button font-semibold border border-secondary-dark transition-colors flex items-center gap-2"
        >
          <X className="w-5 h-5" />
          Cancel
        </button>
      </div>
    </div>
  );
};

export default CreateMeetingForm;
