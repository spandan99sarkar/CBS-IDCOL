import { Entity, Column, PrimaryGeneratedColumn, OneToMany, CreateDateColumn } from 'typeorm';
import { Version } from './version.entity';

@Entity('PROJECTS')
export class Project {
  @PrimaryGeneratedColumn()
  id: number;

  @Column({ name: 'PROJECT_NAME' })
  projectName: string;

  @Column({ name: 'CURRENCY' })
  currency: string;

  @Column({ name: 'LOAN_AMOUNT', type: 'number' })
  loanAmount: number;

  @CreateDateColumn({ name: 'CREATED_AT' })
  createdAt: Date;

  @OneToMany(() => Version, version => version.project)
  versions: Version[];
}
