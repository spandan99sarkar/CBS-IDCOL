'use client';

import React, { useState } from 'react';
import { ChevronDown } from 'lucide-react';
import { cn } from '@/lib/utils';

interface AccordionProps {
  title: string;
  children: React.ReactNode;
  defaultOpen?: boolean;
  id: string;
}

export const Accordion: React.FC<AccordionProps> = ({ title, children, defaultOpen = false, id }) => {
  const [isOpen, setIsOpen] = useState(defaultOpen);

  return (
    <div className="border-b border-gray-100 last:border-0">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="w-full flex items-center justify-between px-4 py-3 bg-gray-50/50 hover:bg-gray-100/80 transition-colors"
      >
        <span className="text-navy font-bold text-[11px] uppercase tracking-wider">{title}</span>
        <ChevronDown 
          size={14} 
          className={cn("text-gray-400 transition-transform duration-200", isOpen && "rotate-180")} 
        />
      </button>
      <div className={cn("overflow-hidden transition-all duration-200", isOpen ? "max-h-[2000px] opacity-100" : "max-h-0 opacity-0")}>
        <div className="p-4 space-y-4">
          {children}
        </div>
      </div>
    </div>
  );
};
