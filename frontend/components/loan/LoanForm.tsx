'use client';

import React from 'react';
import { Form, Input, Select, InputNumber, DatePicker, Checkbox, Row, Col, Button } from 'antd';
import { Plus, Trash2, Info } from 'lucide-react';
import { Accordion } from '@/components/ui/Accordion';
import { useLoanStore } from '@/lib/store';

const { Option } = Select;

export const LoanForm = () => {
  const { params, setParams } = useLoanStore();
  const [form] = Form.useForm();

  // Sync form with store params (e.g. when an example is loaded)
  React.useEffect(() => {
    form.setFieldsValue(params);
  }, [params, form]);

  const handleValuesChange = (_: any, allValues: any) => {
    setParams(allValues);
  };

  return (
    <Form
      form={form}
      layout="vertical"
      initialValues={params}
      onValuesChange={handleValuesChange}
      size="small"
      className="divide-y divide-gray-100"
    >
      <Accordion title="① Loan Parameters" id="params" defaultOpen>
        <Form.Item label="Borrower / Project Name" name="project_name">
          <Input placeholder="e.g. Esquire Knit Composite Ltd" />
        </Form.Item>
        
        <Row gutter={8}>
          <Col span={12}>
            <Form.Item label="Currency" name="currency">
              <Select>
                <Option value="BDT">BDT</Option>
                <Option value="USD">USD</Option>
              </Select>
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="Sanctioned Amount" name="loan_amount">
              <InputNumber 
                className="w-full" 
                placeholder="0.00" 
                formatter={value => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')} 
              />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={8}>
          <Col span={12}>
            <Form.Item label="Agreement / Fin. Close" name="financial_close">
              <DatePicker className="w-full" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="Interest Rate (% p.a.)" name="interest_rate">
              <InputNumber className="w-full" placeholder="10.00" />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={6}>
          <Col span={8}>
            <Form.Item label="Day Basis" name="day_count_basis">
              <Select>
                <Option value={360}>360</Option>
                <Option value={365}>365</Option>
              </Select>
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item label="Frequency" name="payment_frequency">
              <Select>
                <Option value={4}>Quarterly</Option>
                <Option value={12}>Monthly</Option>
                <Option value={2}>Semi-Annual</Option>
                <Option value={1}>Annual</Option>
              </Select>
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item label="# Installments" name="num_installments">
              <InputNumber className="w-full" placeholder="28" />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item label="Principal Type" name="principal_type">
          <Select>
            <Option value="Level Principal">Level Principal</Option>
            <Option value="Annuity">Annuity</Option>
            <Option value="PPMT Principal">PPMT Principal</Option>
            <Option value="Scheduled Principal">Scheduled Principal</Option>
            <Option value="Scheduled Percentage Principal">Scheduled Percentage Principal</Option>
          </Select>
        </Form.Item>
      </Accordion>

      <Accordion title="② Disbursements" id="disb" defaultOpen>
        <Form.List name="disbursements">
          {(fields, { add, remove }) => (
            <>
              {fields.map(({ key, name, ...restField }) => (
                <Row key={key} gutter={8} align="middle" className="mb-2">
                  <Col span={10}>
                    <Form.Item {...restField} name={[name, 'date']} noStyle>
                      <DatePicker className="w-full" placeholder="Date" />
                    </Form.Item>
                  </Col>
                  <Col span={10}>
                    <Form.Item {...restField} name={[name, 'amount']} noStyle>
                      <InputNumber className="w-full" placeholder="Amount" />
                    </Form.Item>
                  </Col>
                  <Col span={4}>
                    <Button 
                      type="text" 
                      danger 
                      icon={<Trash2 size={14} />} 
                      onClick={() => remove(name)} 
                    />
                  </Col>
                </Row>
              ))}
              <Button 
                type="dashed" 
                onClick={() => add({ date: null, amount: null, note: `DD ${fields.length + 1}` })} 
                block 
                icon={<Plus size={14} />}
                className="text-xs border-dashed border-gray-300 hover:border-blue-400 hover:text-blue-500"
              >
                Add Disbursement
              </Button>
            </>
          )}
        </Form.List>
        <div className="text-[10px] text-gray-400 mt-2 italic leading-tight">
          First disbursement is the interest start date. Multiple drawdowns are segmented day-by-day.
        </div>
      </Accordion>

      <Accordion title="③ Grace & Options" id="grace">
        <Row gutter={8}>
          <Col span={12}>
            <Form.Item label="Int. Grace (mo)" name="interest_grace_months">
              <InputNumber className="w-full" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="Prin. Grace (mo)" name="principal_grace_months">
              <InputNumber className="w-full" />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={8}>
          <Col span={12}>
            <Form.Item label="Int. Grace End" name="interest_grace_period_end">
              <DatePicker className="w-full" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="Prin. Grace End" name="principal_grace_period_end">
              <DatePicker className="w-full" />
            </Form.Item>
          </Col>
        </Row>
        <Form.Item name="interest_capitalized" valuePropName="checked" noStyle>
          <Checkbox className="text-[12px]">Capitalize interest during grace</Checkbox>
        </Form.Item>
        <Form.Item label="Capitalize Until" name="interest_capitalized_until" className="mt-2 mb-2">
          <DatePicker className="w-full" />
        </Form.Item>
        <Form.Item name="opening_balance_includes_period_disbursements" valuePropName="checked" className="mt-2 mb-0">
          <Checkbox className="text-[12px]">Opening bal. includes period disb.</Checkbox>
        </Form.Item>
        <div className="mt-3 space-y-1">
          <Form.Item name="annuity_recalculate_on_rate_or_disbursement" valuePropName="checked" noStyle>
            <Checkbox className="text-[11px]">Recalc annuity on rate/disb</Checkbox>
          </Form.Item><br/>
          <Form.Item name="annuity_use_period_rate" valuePropName="checked" noStyle>
            <Checkbox className="text-[11px]">Annuity uses period rate</Checkbox>
          </Form.Item><br/>
          <Form.Item name="total_debt_service_includes_capitalized_interest" valuePropName="checked" noStyle>
            <Checkbox className="text-[11px]">TDS includes cap. interest</Checkbox>
          </Form.Item>
        </div>
      </Accordion>

      <Accordion title="④ Interest Rates" id="rates">
        <Form.List name="interest_rate_change_events">
          {(fields, { add, remove }) => (
            <>
              {fields.map(({ key, name, ...restField }) => (
                <Row key={key} gutter={8} align="middle" className="mb-2">
                  <Col span={12}>
                    <Form.Item {...restField} name={[name, 'date']} noStyle>
                      <DatePicker className="w-full" placeholder="Effective" />
                    </Form.Item>
                  </Col>
                  <Col span={8}>
                    <Form.Item {...restField} name={[name, 'rate']} noStyle>
                      <InputNumber className="w-full" placeholder="%" />
                    </Form.Item>
                  </Col>
                  <Col span={4}>
                    <Button type="text" danger icon={<Trash2 size={14} />} onClick={() => remove(name)} />
                  </Col>
                </Row>
              ))}
              <Button type="dashed" onClick={() => add({ date: null, rate: null })} block icon={<Plus size={14} />} className="text-xs">
                Add Rate Change
              </Button>
            </>
          )}
        </Form.List>
      </Accordion>

      <Accordion title="⑤ Repayment Dates" id="repayment">
        <Form.Item label="Repayment Date Mode" name="gen_mode">
          <Select>
            <Option value="auto">Auto-generate from rules</Option>
            <Option value="manual">Manual list</Option>
          </Select>
        </Form.Item>

        <Form.Item noStyle shouldUpdate={(prev, curr) => prev.gen_mode !== curr.gen_mode}>
          {({ getFieldValue }) => 
            getFieldValue('gen_mode') === 'auto' ? (
              <Row gutter={8}>
                <Col span={12}>
                  <Form.Item label="First Payment" name="first_payment">
                    <DatePicker className="w-full" />
                  </Form.Item>
                </Col>
                <Col span={12}>
                  <Form.Item label="Payment Day" name="payment_day">
                    <InputNumber className="w-full" placeholder="15" />
                  </Form.Item>
                </Col>
              </Row>
            ) : null
          }
        </Form.Item>

        <Form.Item label="Business-Day Rule" name="bd_rule">
          <Select>
            <Option value="none">No adjustment</Option>
            <Option value="preceding">Preceding business day</Option>
            <Option value="succeeding">Succeeding business day</Option>
          </Select>
        </Form.Item>
      </Accordion>

      <Accordion title="⑥ Principal Schedule" id="psched">
        <div className="text-[10px] text-gray-400 mb-2 italic">
          For 'Scheduled Principal' enter amounts. For 'Scheduled Percentage' enter % (e.g. 2.25).
        </div>
        <Form.Item noStyle shouldUpdate={(prev, curr) => prev.principal_type !== curr.principal_type}>
          {({ getFieldValue }) => {
            const isPct = getFieldValue('principal_type') === 'Scheduled Percentage Principal';
            const name = isPct ? 'principal_schedule_percentages' : 'principal_schedule_amounts';
            return (
              <Form.List name={name}>
                {(fields, { add, remove }) => (
                  <>
                    {fields.map(({ key, name: n, ...restField }) => (
                      <Row key={key} gutter={8} align="middle" className="mb-2">
                        <Col span={4} className="text-[10px] text-gray-400 font-bold">#{n+1}</Col>
                        <Col span={16}>
                          <Form.Item {...restField} name={n} noStyle>
                            <InputNumber className="w-full" placeholder={isPct ? '%' : 'Amount'} />
                          </Form.Item>
                        </Col>
                        <Col span={4}>
                          <Button type="text" danger icon={<Trash2 size={14} />} onClick={() => remove(n)} />
                        </Col>
                      </Row>
                    ))}
                    <Button type="dashed" onClick={() => add()} block icon={<Plus size={14} />} className="text-xs">
                      Add Row
                    </Button>
                  </>
                )}
              </Form.List>
            );
          }}
        </Form.Item>
      </Accordion>
    </Form>
  );
};
