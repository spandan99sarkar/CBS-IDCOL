import { Entity, Column, PrimaryGeneratedColumn, ManyToOne, CreateDateColumn } from 'typeorm';
import { Project } from './project.entity';

@Entity('VERSIONS')
export class Version {
  @PrimaryGeneratedColumn()
  id: number;

  @Column({ name: 'DESCRIPTION' })
  description: string;

  @Column({ name: 'TAG' })
  tag: string;

  @Column({ name: 'FORM_DATA', type: 'clob' })
  formData: string;

  @Column({ name: 'PARAMS_DATA', type: 'clob' })
  paramsData: string;

  @Column({ name: 'SCHEDULE_DATA', type: 'clob' })
  scheduleData: string;

  @CreateDateColumn({ name: 'CREATED_AT' })
  createdAt: Date;

  @ManyToOne(() => Project, project => project.versions)
  project: Project;
}
