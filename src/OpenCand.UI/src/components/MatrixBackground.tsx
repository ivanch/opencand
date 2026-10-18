import React, { useEffect, useRef, useState } from 'react';
import { createAnimationLoop, type AnimationLoop } from '../utils/animationLoop';

interface Dot {
  x: number;
  y: number;
  vx: number;
  vy: number;
  connections: number[];
  brightness: number;
  targetBrightness: number;
}

interface Connection {
  from: number;
  to: number;
  brightness: number;
  targetBrightness: number;
  thickness: number;
  targetThickness: number;
}

const MatrixBackground: React.FC = () => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const loopRef = useRef<AnimationLoop | null>(null);
  const dotsRef = useRef<Dot[]>([]);
  const connectionsRef = useRef<Connection[]>([]);
  const mouseRef = useRef({ x: 0, y: 0 });
  const [, setDimensions] = useState({ width: 0, height: 0 });

  const isMobile = /iPad|iPhone|iPod|Android/i.test(navigator.userAgent);

  // Configuration - subtle background for main content
  const config = {
    dotCount: 400,
    maxConnections: 3,
    connectionDistance: 150,
    dotSpeed: 0.3,
    hoverRadius: 120,
    baseBrightness: 0.2, // Moderate base brightness for main background
    hoverBrightness: 0.5, // Moderate hover brightness
    baseThickness: 0.6,
    hoverThickness: 1.8,
    fadeSpeed: 0.08, // Slightly faster fade for better responsiveness
  };

  // Initialize dots
  const initializeDots = (width: number, height: number) => {
    const dots: Dot[] = [];
    for (let i = 0; i < config.dotCount; i++) {
      dots.push({
        x: Math.random() * width,
        y: Math.random() * height,
        vx: (Math.random() - 0.5) * config.dotSpeed,
        vy: (Math.random() - 0.5) * config.dotSpeed,
        connections: [],
        brightness: config.baseBrightness,
        targetBrightness: config.baseBrightness,
      });
    }
    return dots;
  };

  // Calculate connections between dots
  const calculateConnections = (dots: Dot[]) => {
    const connections: Connection[] = [];
    
    for (let i = 0; i < dots.length; i++) {
      const dot = dots[i];
      dot.connections = [];
      
      const distances: { index: number; distance: number }[] = [];
      
      for (let j = 0; j < dots.length; j++) {
        if (i === j) continue;
        
        const dx = dot.x - dots[j].x;
        const dy = dot.y - dots[j].y;
        const distance = Math.sqrt(dx * dx + dy * dy);
        
        if (distance < config.connectionDistance) {
          distances.push({ index: j, distance });
        }
      }
      
      // Sort by distance and take closest connections
      distances.sort((a, b) => a.distance - b.distance);
      const maxConnections = Math.min(config.maxConnections, distances.length);
      
      for (let k = 0; k < maxConnections; k++) {
        const targetIndex = distances[k].index;
        
        // Avoid duplicate connections
        const existing = connections.find(
          conn => (conn.from === i && conn.to === targetIndex) || 
                  (conn.from === targetIndex && conn.to === i)
        );
        
        if (!existing) {
          dot.connections.push(targetIndex);
          connections.push({
            from: i,
            to: targetIndex,
            brightness: config.baseBrightness,
            targetBrightness: config.baseBrightness,
            thickness: config.baseThickness,
            targetThickness: config.baseThickness,
          });
        }
      }
    }
    
    return connections;
  };

  // Update hover effects
  const updateHoverEffects = (dots: Dot[], connections: Connection[], mouseX: number, mouseY: number) => {
    // Reset all targets to base values
    dots.forEach(dot => {
      dot.targetBrightness = config.baseBrightness;
    });
    
    connections.forEach(conn => {
      conn.targetBrightness = config.baseBrightness;
      conn.targetThickness = config.baseThickness;
    });

    // Check which dots are near the mouse
    dots.forEach((dot, index) => {
      const dx = dot.x - mouseX;
      const dy = dot.y - mouseY;
      const distance = Math.sqrt(dx * dx + dy * dy);
      
      if (distance < config.hoverRadius) {
        const intensity = 1 - (distance / config.hoverRadius);
        dot.targetBrightness = config.baseBrightness + (config.hoverBrightness - config.baseBrightness) * intensity;
        
        // Enhance connected dots and their connections
        dot.connections.forEach(connIndex => {
          if (dots[connIndex]) {
            dots[connIndex].targetBrightness = Math.max(
              dots[connIndex].targetBrightness,
              config.baseBrightness + (config.hoverBrightness - config.baseBrightness) * intensity * 0.5
            );
          }
          
          // Find and enhance the connection
          const connection = connections.find(
            conn => (conn.from === index && conn.to === connIndex) || 
                    (conn.from === connIndex && conn.to === index)
          );
          if (connection) {
            connection.targetBrightness = Math.max(
              connection.targetBrightness,
              config.baseBrightness + (config.hoverBrightness - config.baseBrightness) * intensity
            );
            connection.targetThickness = Math.max(
              connection.targetThickness,
              config.baseThickness + (config.hoverThickness - config.baseThickness) * intensity
            );
          }
        });
      }
    });

    // Smooth transitions
    dots.forEach(dot => {
      dot.brightness += (dot.targetBrightness - dot.brightness) * config.fadeSpeed;
    });
    
    connections.forEach(conn => {
      conn.brightness += (conn.targetBrightness - conn.brightness) * config.fadeSpeed;
      conn.thickness += (conn.targetThickness - conn.thickness) * config.fadeSpeed;
    });
  };

  // Animation loop
  const animate = () => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const { width, height } = canvas;
    
    // Clear canvas with dark blue background
    ctx.fillStyle = '#0a0f1a';
    ctx.fillRect(0, 0, width, height);

    const dots = dotsRef.current;
    const connections = connectionsRef.current;

    // Update dot positions
    dots.forEach(dot => {
      dot.x += dot.vx;
      dot.y += dot.vy;

      // Bounce off edges
      if (dot.x <= 0 || dot.x >= width) {
        dot.vx = -dot.vx;
        dot.x = Math.max(0, Math.min(width, dot.x));
      }
      if (dot.y <= 0 || dot.y >= height) {
        dot.vy = -dot.vy;
        dot.y = Math.max(0, Math.min(height, dot.y));
      }
    });

    // Recalculate connections periodically for smooth movement
    if (Math.random() < 0.02) {
      connectionsRef.current = calculateConnections(dots);
    }

    // Update hover effects
    updateHoverEffects(dots, connections, mouseRef.current.x, mouseRef.current.y);

    // Draw connections with moderate opacity for main background
    connections.forEach(conn => {
      const fromDot = dots[conn.from];
      const toDot = dots[conn.to];
      
      if (!fromDot || !toDot) return;

      ctx.strokeStyle = `rgba(100, 200, 255, ${conn.brightness * 0.25})`;
      ctx.lineWidth = conn.thickness;
      ctx.lineCap = 'round';
      
      ctx.beginPath();
      ctx.moveTo(fromDot.x, fromDot.y);
      ctx.lineTo(toDot.x, toDot.y);
      ctx.stroke();
    });

    // Draw dots with moderate opacity for main background
    dots.forEach(dot => {
      ctx.fillStyle = `rgba(100, 200, 255, ${dot.brightness * 0.25})`;
      ctx.beginPath();
      ctx.arc(dot.x, dot.y, 2, 0, Math.PI * 2);
      ctx.fill();
      
      // Add subtle glow
      ctx.shadowColor = `rgba(100, 200, 255, ${dot.brightness * 0.12})`;
      ctx.shadowBlur = 4;
      ctx.beginPath();
      ctx.arc(dot.x, dot.y, 1.5, 0, Math.PI * 2);
      ctx.fill();
      ctx.shadowBlur = 0;
    });

  };

  // Handle resize
  const handleResize = () => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    // Avoid unnecessary updates for performance reasons on mobile
    if (isMobile) return;

    // Set canvas size to window size
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
    
    canvas.style.width = `${window.innerWidth}px`;
    canvas.style.height = `${window.innerHeight}px`;

    setDimensions({ width: window.innerWidth, height: window.innerHeight });
    
    // Reinitialize dots for new dimensions
    dotsRef.current = initializeDots(window.innerWidth, window.innerHeight);
    connectionsRef.current = calculateConnections(dotsRef.current);
    loopRef.current?.redrawIfStatic();
    
    if (process.env.NODE_ENV === 'development') {
      console.log(`MatrixBackground: Canvas resized to ${window.innerWidth}x${window.innerHeight}`);
    }
  };

  // Handle mouse movement
  const handleMouseMove = (event: MouseEvent) => {
    mouseRef.current = {
      x: event.clientX,
      y: event.clientY,
    };
  };

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) {
      if (process.env.NODE_ENV === 'development') {
        console.log('MatrixBackground: Canvas ref not found');
      }
      return;
    }

    if (process.env.NODE_ENV === 'development') {
      console.log('MatrixBackground: Canvas found, initializing...');
    }

    // Initial setup
    handleResize();
    
    // Start animation: capped at 30fps, paused while the tab is hidden,
    // a single static frame under prefers-reduced-motion
    const loop = createAnimationLoop(animate);
    loopRef.current = loop;
    loop.start();

    // Event listeners
    window.addEventListener('resize', handleResize);
    document.addEventListener('mousemove', handleMouseMove);

    if (process.env.NODE_ENV === 'development') {
      console.log('MatrixBackground: Animation started');
    }

    return () => {
      loop.stop();
      loopRef.current = null;
      window.removeEventListener('resize', handleResize);
      document.removeEventListener('mousemove', handleMouseMove);
      if (process.env.NODE_ENV === 'development') {
        console.log('MatrixBackground: Cleanup completed');
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <canvas
      ref={canvasRef}
      className="fixed inset-0"
      style={{ 
        zIndex: 0,
        position: 'fixed',
        top: 0,
        left: 0,
        width: '100vw',
        height: '100vh',
        backgroundColor: '#0a0f1a', // Dark blue background
        pointerEvents: 'none'
      }}
    />
  );
};

export default MatrixBackground;
