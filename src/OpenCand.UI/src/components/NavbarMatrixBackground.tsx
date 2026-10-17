import React, { useEffect, useRef, useState } from 'react';

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

const NavbarMatrixBackground: React.FC = () => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const animationRef = useRef<number>(0);
  const dotsRef = useRef<Dot[]>([]);
  const connectionsRef = useRef<Connection[]>([]);
  const mouseRef = useRef({ x: 0, y: 0 });
  const [dimensions, setDimensions] = useState({ width: 0, height: 20 }); // Navbar height

  // Configuration optimized for navbar - higher intensity, more visible
  const config = {
    dotCount: 60, // Fewer dots for the smaller navbar area
    maxConnections: 4,
    connectionDistance: 120,
    dotSpeed: 0.2, // Slower movement
    hoverRadius: 100,
    baseBrightness: 0.1, // Much higher base brightness for navbar
    hoverBrightness: 0.2, // Maximum brightness on hover
    baseThickness: 1.0,
    hoverThickness: 2.5,
    fadeSpeed: 0.1,
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

    // Check which dots are near the mouse (only if mouse is in navbar area)
    if (mouseY <= 80) { // Navbar height
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
                config.baseBrightness + (config.hoverBrightness - config.baseBrightness) * intensity * 0.7
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
    }

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
    
    // Clear canvas with transparent background
    ctx.clearRect(0, 0, width, height);

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
    if (Math.random() < 0.05) { // More frequent recalculation for smaller area
      connectionsRef.current = calculateConnections(dots);
    }

    // Update hover effects
    updateHoverEffects(dots, connections, mouseRef.current.x, mouseRef.current.y);

    // Draw connections with higher opacity for navbar visibility
    connections.forEach(conn => {
      const fromDot = dots[conn.from];
      const toDot = dots[conn.to];
      
      if (!fromDot || !toDot) return;

      ctx.strokeStyle = `rgba(100, 200, 255, ${conn.brightness * 0.4})`;
      ctx.lineWidth = conn.thickness;
      ctx.lineCap = 'round';
      
      ctx.beginPath();
      ctx.moveTo(fromDot.x, fromDot.y);
      ctx.lineTo(toDot.x, toDot.y);
      ctx.stroke();
    });

    // Draw dots with higher opacity for navbar visibility
    dots.forEach(dot => {
      ctx.fillStyle = `rgba(100, 200, 255, ${dot.brightness * 0.5})`;
      ctx.beginPath();
      ctx.arc(dot.x, dot.y, 1.5, 0, Math.PI * 2);
      ctx.fill();
      
      // Add brighter glow for navbar
      ctx.shadowColor = `rgba(100, 200, 255, ${dot.brightness * 0.3})`;
      ctx.shadowBlur = 3;
      ctx.beginPath();
      ctx.arc(dot.x, dot.y, 1, 0, Math.PI * 2);
      ctx.fill();
      ctx.shadowBlur = 0;
    });

    animationRef.current = requestAnimationFrame(animate);
  };

  // Handle resize
  const handleResize = () => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const navbarHeight = 80; // Fixed navbar height
    canvas.width = window.innerWidth;
    canvas.height = navbarHeight;
    
    canvas.style.width = `${window.innerWidth}px`;
    canvas.style.height = `${navbarHeight}px`;

    setDimensions({ width: window.innerWidth, height: navbarHeight });
    
    // Reinitialize dots for new dimensions
    dotsRef.current = initializeDots(window.innerWidth, navbarHeight);
    connectionsRef.current = calculateConnections(dotsRef.current);
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
    if (!canvas) return;

    // Initial setup
    handleResize();
    
    // Start animation
    const startAnimation = () => {
      if (animationRef.current) {
        cancelAnimationFrame(animationRef.current);
      }
      animationRef.current = requestAnimationFrame(animate);
    };
    
    startAnimation();

    // Event listeners
    window.addEventListener('resize', handleResize);
    document.addEventListener('mousemove', handleMouseMove);

    return () => {
      if (animationRef.current) {
        cancelAnimationFrame(animationRef.current);
      }
      window.removeEventListener('resize', handleResize);
      document.removeEventListener('mousemove', handleMouseMove);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <canvas
      ref={canvasRef}
      className="absolute inset-0 w-full h-full"
      style={{ 
        zIndex: 1,
        pointerEvents: 'none'
      }}
    />
  );
};

export default NavbarMatrixBackground;
