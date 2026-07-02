import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { EngineController } from './engine/engine.controller';
import { EngineService } from './engine/engine.service';
import { Project } from './entities/project.entity';
import { Version } from './entities/version.entity';

@Module({
  imports: [
    /*
    TypeOrmModule.forRoot({
      type: 'oracle',
      host: 'localhost',
      port: 1521,
      username: 'your_username',
      password: 'your_password',
      sid: 'xe',
      entities: [Project, Version],
      synchronize: true,
    }),
    TypeOrmModule.forFeature([Project, Version]),
    */
  ],
  controllers: [EngineController],
  providers: [EngineService],
})
export class AppModule {}
