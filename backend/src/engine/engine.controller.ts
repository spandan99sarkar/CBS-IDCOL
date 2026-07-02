import { Controller, Post, Body } from '@nestjs/common';
import { EngineService, LoanParams, ScheduleRow } from './engine.service';

@Controller('engine')
export class EngineController {
  constructor(private readonly engineService: EngineService) {}

  @Post('compute')
  computeSchedule(@Body() params: LoanParams): ScheduleRow[] {
    console.log('Received compute request for project:', params.project_name);
    try {
      const result = this.engineService.generateSchedule(params);
      console.log('Successfully computed schedule with', result.length, 'periods');
      return result;
    } catch (error) {
      console.error('Error computing schedule:', error);
      throw error;
    }
  }
}
