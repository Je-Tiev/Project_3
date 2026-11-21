import React, { useState, useEffect } from 'react';
import { formatTime } from '../utils/dateUtils';

const LiveClock = ({ className = 'text-sm' }) => {
  const [time, setTime] = useState(new Date());

  useEffect(() => {
    const timer = setInterval(() => setTime(new Date()), 1000);
    return () => clearInterval(timer);
  }, []);

  return <span className={className}>{formatTime(time)}</span>;
};

export default LiveClock;
